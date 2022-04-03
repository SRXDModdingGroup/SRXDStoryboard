﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StoryboardSystem;

internal static class Compiler {
    public static bool TryCompileFile(string path, ITimeConversion timeConversion, out Storyboard storyboard) {
        var logger = StoryboardManager.Instance.Logger;
        
        if (!Parser.TryParseFile(path, out var instructions))
            logger.LogWarning($"Failed to parse {path}");
        else if (!TryCompile(instructions, timeConversion, logger, out storyboard))
            logger.LogWarning($"Failed to compile {path}");
        else {
            logger.LogMessage($"Successfully compiled {path}");
            
            return true;
        }

        storyboard = null;
            
        return false;
    }

    private static bool TryCompile(List<Instruction> instructions, ITimeConversion timeConversion, ILogger logger, out Storyboard storyboard) {
        storyboard = null;

        var assetBundleReferences = new List<LoadedAssetBundleReference>();
        var assetReferences = new List<LoadedAssetReference>();
        var instanceReferences = new List<LoadedInstanceReference>();
        var postProcessReferences = new List<LoadedPostProcessingMaterialReference>();
        var eventBuilders = new Dictionary<Binding, EventBuilder>();
        var curveBuilders = new Dictionary<Binding, CurveBuilder>();
        var procedures = new Dictionary<Name, Procedure>();
        var globals = new Dictionary<Name, object>();
        var globalScope = new Scope(null, 0, 0, 0, Timestamp.Zero, Timestamp.Zero, globals, null);
        bool inProcs = false;

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;

            switch (opcode) {
                case Opcode.Bundle when TryGetArguments(arguments, globalScope, out Name name, out string path):
                    var newAssetBundleReference = new LoadedAssetBundleReference(path);

                    assetBundleReferences.Add(newAssetBundleReference);
                    globals[name] = newAssetBundleReference;

                    break;
                case Opcode.Inst when TryGetArguments(arguments, globalScope, out Name name, out LoadedAssetReference assetReference):
                    var newInstanceReference = assetReference.CreateInstanceReference();
                    
                    instanceReferences.Add(newInstanceReference);
                    globals[name] = newInstanceReference;
                    
                    break;
                case Opcode.Load when TryGetArguments(arguments, globalScope, out Name name, out AssetType type, out LoadedAssetBundleReference assetBundleReference, out string assetName):
                    var newAssetReference = LoadedAssetReference.Create(assetBundleReference, assetName, type);
                    
                    assetReferences.Add(newAssetReference);
                    globals[name] = newAssetReference;
                    
                    break;
                case Opcode.Post when TryGetArguments(arguments, globalScope, out LoadedInstanceReference<Material> material, out int layer):
                    postProcessReferences.Add(new LoadedPostProcessingMaterialReference(material, layer));
                    
                    break;
                case Opcode.Proc when TryGetArguments(arguments, globalScope, out Name name, true):
                    if (procedures.ContainsKey(name)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, $"Procedure {name} already exists"));
                
                        return false;
                    }
                    
                    var argNames = new Name[arguments.Length - 1];

                    for (int j = 1, k = 0; j < arguments.Length; j++, k++) {
                        if (arguments[j] is not Name argName) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, "Invalid arguments for instruction Proc"));

                            return false;
                        }

                        if (Array.Exists(argNames, n => n == argName)) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, $"Argument name {argName} already exists"));

                            return false;
                        }

                        argNames[k] = argName;
                    }

                    procedures.Add(name, new Procedure(i, argNames));
                    inProcs = true;
                    
                    break;
                case Opcode.SetA when TryGetArguments(arguments, globalScope, out Index idx, out object value):
                    if (!inProcs)
                        idx.Array[idx.index] = value;

                    break;
                case Opcode.SetG when TryGetArguments(arguments, globalScope, out Name name, out object value):
                    if (!inProcs)
                        globals[name] = value;

                    break;
                case Opcode.Call:
                case Opcode.Event:
                case Opcode.Key:
                case Opcode.Loop:
                case Opcode.Set:
                    if (inProcs)
                        break;

                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} must be used within a procedure"));

                    return false;
                default:
                    var builder = new StringBuilder();

                    for (int j = 0; j < arguments.Length; j++) {
                        builder.Append(arguments[j].GetType().Name);

                        if (j < arguments.Length - 1)
                            builder.Append(", ");
                    }
                    
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}. Types found: {builder.ToString()}"));

                    return false;
            }
        }

        if (!procedures.TryGetValue(new Name("Main"), out var procedure)) {
            logger.LogWarning("Failed to compile storyboard: Procedure Main could not be found");

            return false;
        }

        int index1 = procedure.StartIndex;
        int orderCounter = 0;
        var currentScope = new Scope(null, index1, 0, 1, Timestamp.Zero, Timestamp.Zero, globals, new Dictionary<Name, object>());
        var iterName = new Name("iter");

        while (currentScope != null) {
            index1++;
            
            if (index1 >= instructions.Count || instructions[index1].Opcode == Opcode.Proc) {
                if (currentScope.NextIteration())
                    index1 = currentScope.StartIndex;
                else {
                    index1 = currentScope.ReturnIndex;
                    currentScope = currentScope.Parent;
                }

                continue;
            }

            globals[iterName] = currentScope.CurrentIteration;
            
            var instruction = instructions[index1];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;
            
            switch (opcode) {
                case Opcode.Call when TryGetArguments(arguments, currentScope, out Timestamp time, out Name name, true):
                    if (!TryCallProcedure(time, name, 2, 1, Timestamp.Zero))
                        return false;

                    break;
                case Opcode.Event when TryGetArguments(arguments, currentScope, out Timestamp time, out Binding binding):
                    if (!eventBuilders.TryGetValue(binding, out var eventBuilder)) {
                        eventBuilder = new EventBuilder();
                        eventBuilders.Add(binding, eventBuilder);
                    }
                    
                    eventBuilder.AddTime(currentScope.GetGlobalTime(time));
                    
                    break;
                case Opcode.Key when TryGetArguments(arguments, currentScope, out Timestamp time, out Binding binding, out VectorN value, out InterpType interpType):
                    if (!curveBuilders.TryGetValue(binding, out var curveBuilder)) {
                        curveBuilder = new CurveBuilder();
                        curveBuilders.Add(binding, curveBuilder);
                    }

                    curveBuilder.AddKey(currentScope.GetGlobalTime(time), value, interpType, orderCounter);
                    
                    break;
                case Opcode.Loop when TryGetArguments(arguments, currentScope, out Timestamp time, out Name name, out int iterations, out Timestamp every, true):
                    if (!TryCallProcedure(time, name, 4, iterations, every))
                        return false;
                    
                    break;
                case Opcode.Set when TryGetArguments(arguments, currentScope, out Name name, out object value):
                    currentScope.SetValue(name, value);

                    break;
                case Opcode.SetA when TryGetArguments(arguments, currentScope, out Index idx, out object value):
                    if (!inProcs)
                        idx.Array[idx.index] = value;

                    break;
                case Opcode.SetG when TryGetArguments(arguments, currentScope, out Name name, out object value):
                    globals[name] = value;

                    break;
                case Opcode.Bundle:
                case Opcode.Inst:
                case Opcode.Load:
                case Opcode.Post:
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure"));

                    return false;
                case Opcode.Proc:
                default:
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}"));

                    return false;
            }

            bool TryCallProcedure(Timestamp time, Name name, int shift, int iterations, Timestamp every) {
                if (iterations <= 0) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Iterations must be greater than 0"));

                    return false;
                }
                
                if (!procedures.TryGetValue(name, out procedure)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Procedure {name} could not be found"));

                    return false;
                }

                var argNames = procedure.ArgNames;

                if (arguments.Length != argNames.Length + shift) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {name}"));

                    return false;
                }

                int newIndex = procedure.StartIndex;

                if (!currentScope.CheckForRecursion(newIndex)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Recursive procedure call detected"));

                    return false;
                }

                var locals = new Dictionary<Name, object>();

                for (int i = shift, j = 0; i < arguments.Length; i++, j++)
                    locals.Add(argNames[j], arguments[i]);

                currentScope = new Scope(currentScope, newIndex, index1, iterations, currentScope.GetGlobalTime(time), every, globals, locals);
                index1 = newIndex;
                
                return true;
            }
        }
        
        storyboard = new Storyboard(timeConversion, assetBundleReferences.ToArray(), assetReferences.ToArray(), instanceReferences.ToArray(), postProcessReferences.ToArray(), eventBuilders, curveBuilders);

        return true;
    }

    private static bool TryResolveArgument<T>(object argument, Scope scope, out T value) {
        value = default;
        
        switch (argument) {
            case Name name when typeof(T) != typeof(Name): {
                if (!scope.TryGetValue(name, out argument))
                    return false;

                break;
            }
            case object[] arr: {
                object[] newArr = new object[arr.Length];
            
                for (int i = 0; i < arr.Length; i++) {
                    if (!TryResolveArgument(arr[i], scope, out newArr[i]))
                        return false;
                }

                argument = newArr;

                break;
            }
            case Chain chain: {
                if (chain[0] is not Name name0 || !scope.TryGetValue(name0, out argument))
                    return false;
                
                object[] sequence = new object[chain.Length - 1];

                for (int i = 1, j = 0; i < chain.Length; i++, j++) {
                    object node = chain[i];

                    switch (node) {
                        case Indexer indexer: {
                            if (!TryResolveArgument(indexer.Token, scope, out int index0))
                                return false;

                            sequence[j] = index0;
                        
                            continue;
                        }
                        case Name name1:
                            sequence[j] = name1.ToString();
                        
                            continue;
                    }
                
                    return false;
                }

                for (int i = 0; i < sequence.Length; i++) {
                    if (argument is Index index0) {
                        if (index0.index < 0 || index0.index >= index0.Array.Length)
                            return false;
                    
                        argument = index0.Array[index0.index];
                    }
                
                    switch (sequence[i]) {
                        case int index1 when argument is object[] arr: {
                            argument = new Index(arr, index1);
                        
                            continue;
                        }
                        case string:
                        case int: {
                            object[] newSequence;
                            LoadedObjectReference reference0;
                            int j;

                            switch (argument) {
                                case LoadedObjectReference reference1:
                                    newSequence = new object[sequence.Length - i];
                                    reference0 = reference1;
                                    j = 0;

                                    break;
                                case Binding binding:
                                    object[] oldSequence = binding.Sequence;
                                    
                                    newSequence = new object[oldSequence.Length + sequence.Length - i];
                                    oldSequence.CopyTo(newSequence, 0);
                                    reference0 = binding.Reference;
                                    j = oldSequence.Length;

                                    break;
                                default:
                                    return false;
                            }

                            for (; i < sequence.Length; i++, j++)
                                newSequence[j] = sequence[i];
                            
                            argument = new Binding(reference0, newSequence);
                            
                            continue;
                        }
                    }
                
                    return false;
                }
                
                if (argument is Index index2 && typeof(T) != typeof(Index)) {
                    if (index2.index < 0 || index2.index >= index2.Array.Length)
                        return false;

                    argument = index2.Array[index2.index];
                }

                break;
            }
        }

        if (typeof(T) == typeof(VectorN)) {
            if (argument is object[] arr) {
                if (arr.Length is 0 or > 4)
                    return false;
                
                float x = 0f;
                float y = 0f;
                float z = 0f;
                float w = 0f;
                int dimensions = arr.Length;

                if (dimensions >= 1 && !TryConvertToFloat(arr[0], out x)
                    || dimensions >= 2 && !TryConvertToFloat(arr[1], out y)
                    || dimensions >= 3 && !TryConvertToFloat(arr[2], out z)
                    || dimensions >= 4 && !TryConvertToFloat(arr[3], out w))
                    return false;

                argument = new VectorN(new Vector4(x, y, z, w), dimensions);
            }
            else if (TryConvertToFloat(argument, out float x))
                argument = new VectorN(new Vector4(x, 0f, 0f, 0f), 1);

            bool TryConvertToFloat(object obj, out float f) {
                switch (obj) {
                    case float floatVal:
                        f = floatVal;

                        return true;
                    case int intVal:
                        f = intVal;

                        return true;
                    case bool boolVal:
                        f = boolVal ? 1f : 0f;

                        return true;
                    default:
                        f = default;

                        return false;
                }
            }
        }

        if (argument is not T result)
            return false;
        
        value = result;

        return true;
    }

    private static bool TryGetArguments<T>(object[] arguments, Scope scope, out T arg, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 1 : arguments.Length == 1)
            && TryResolveArgument(arguments[0], scope, out arg))
            return true;

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 2 : arguments.Length == 2)
            && TryResolveArgument(arguments[0], scope, out arg0)
            && TryResolveArgument(arguments[1], scope, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 3 : arguments.Length == 3)
            && TryResolveArgument(arguments[0], scope, out arg0)
            && TryResolveArgument(arguments[1], scope, out arg1)
            && TryResolveArgument(arguments[2], scope, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 4 : arguments.Length == 4)
            && TryResolveArgument(arguments[0], scope, out arg0)
            && TryResolveArgument(arguments[1], scope, out arg1)
            && TryResolveArgument(arguments[2], scope, out arg2)
            && TryResolveArgument(arguments[3], scope, out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }

    private static string GetCompileError(int lineIndex, string message) => $"Failed to compile instruction on line {lineIndex}: {message}";
}
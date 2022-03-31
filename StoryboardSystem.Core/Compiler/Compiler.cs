using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core;

internal static class Compiler {
    public static bool TryCompileFile(string path, Action<string> errorCallback, out Storyboard storyboard) {
        if (Parser.TryParseFile(path, errorCallback, out var instructions))
            return TryCompile(instructions, errorCallback, out storyboard);
        
        storyboard = null;
            
        return false;
    }

    private static bool TryCompile(List<Instruction> instructions, Action<string> errorCallback, out Storyboard storyboard) {
        storyboard = null;

        var assetBundleReferences = new List<LoadedAssetBundleReference>();
        var assetReferences = new List<LoadedAssetReference>();
        var instanceReferences = new List<LoadedInstanceReference>();
        var postProcessReferences = new List<LoadedPostProcessingMaterialReference>();
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
                    globals[name] = VariableTree.Create(newInstanceReference);
                    
                    break;
                case Opcode.Load when TryGetArguments(arguments, globalScope, out Name name, out AssetType type, out LoadedAssetBundleReference assetBundleReference, out string assetName):
                    var newAssetReference = LoadedAssetReference.Create(assetBundleReference, assetName, type);
                    
                    assetReferences.Add(newAssetReference);
                    globals[name] = newAssetReference;
                    
                    break;
                case Opcode.Post when TryGetArguments(arguments, globalScope, out LoadedAssetReference<Material> material, out int layer):
                    postProcessReferences.Add(new LoadedPostProcessingMaterialReference(material, layer));
                    
                    break;
                case Opcode.Proc when TryGetArguments(arguments, globalScope, out Name name, true):
                    if (procedures.ContainsKey(name)) {
                        errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Procedure {name} already exists"));
                
                        return false;
                    }
                    
                    var argNames = new Name[arguments.Length - 1];

                    for (int j = 1, k = 0; j < arguments.Length; j++, k++) {
                        if (arguments[j] is not Name argName) {
                            errorCallback?.Invoke(GetCompileError(instruction.LineIndex, "Invalid arguments for instruction Proc"));

                            return false;
                        }

                        if (Array.Exists(argNames, n => n == argName)) {
                            errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Argument name {argName} already exists"));

                            return false;
                        }

                        argNames[k] = argName;
                    }

                    procedures.Add(name, new Procedure(i, argNames));
                    inProcs = true;
                    
                    break;
                case Opcode.Set when TryGetArguments(arguments, globalScope, out Index idx, out object value):
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
                    break;
                default:
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}"));

                    return false;
            }
        }

        if (!procedures.TryGetValue(new Name("Main"), out var procedure)) {
            errorCallback?.Invoke("Failed to compile storyboard: Procedure Main could not be found");

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
                case Opcode.Event:
                    break;
                case Opcode.Key:
                    break;
                case Opcode.Loop when TryGetArguments(arguments, currentScope, out Timestamp time, out Name name, out int iterations, out Timestamp every, true):
                    if (!TryCallProcedure(time, name, 4, iterations, every))
                        return false;
                    
                    break;
                case Opcode.Set when TryGetArguments(arguments, currentScope, out Name name, out object value):
                    currentScope.SetValue(name, value);

                    break;
                
                case Opcode.Set when TryGetArguments(arguments, currentScope, out Index idx, out object value):
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
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure"));

                    return false;
                case Opcode.Proc:
                default:
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}"));

                    return false;
            }

            bool TryCallProcedure(Timestamp time, Name name, int shift, int iterations, Timestamp every) {
                if (iterations <= 0) {
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, "Iterations must be greater than 0"));

                    return false;
                }
                
                if (!procedures.TryGetValue(name, out procedure)) {
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Procedure {name} could not be found"));

                    return false;
                }

                var argNames = procedure.ArgNames;

                if (arguments.Length != argNames.Length + shift) {
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {name}"));

                    return false;
                }

                int newIndex = procedure.StartIndex;

                if (!currentScope.CheckForRecursion(newIndex)) {
                    errorCallback?.Invoke(GetCompileError(instruction.LineIndex, "Recursive procedure call detected"));

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

        storyboard = new Storyboard(assetBundleReferences.ToArray(), assetReferences.ToArray(), instanceReferences.ToArray(), postProcessReferences.ToArray(), null, null);

        return true;
    }

    private static bool TryResolveImmediateOrVariable<T>(object argument, Scope scope, out T value) {
        value = default;

        if (typeof(T) == typeof(Name)) {
            if (argument is not T name0)
                return false;
            
            value = name0;

            return true;
        }

        if (argument is Name name1) {
            if (!scope.TryGetValue(name1, out argument))
                return false;
        }
        else if (argument is Chain chain) {
            if (chain[0] is not Name name2 || !scope.TryGetValue(name2, out argument))
                return false;

            for (int i = 1; i < chain.Length; i++) {
                object node = chain[i];

                if (argument is Index index0)
                    argument = index0.Array[index0.index];

                if (node is Name name3) {
                    if (argument is not VariableTree variable0 || !variable0.TryGetSubVariable(name3, out argument))
                        return false;
                }
                else if (node is Indexer indexer) {
                    if (argument is not object[] arr || !TryResolveImmediateOrVariable(indexer.Token, scope, out int index1))
                        return false;

                    argument = new Index(arr, index1);
                }
                else
                    return false;
            }
        }

        if (typeof(T) == typeof(Index)) {
            if (argument is not T index3)
                return false;

            value = index3;
            
            return true;
        }
        
        if (argument is Index index4)
            argument = index4.Array[index4.index];

        return Conversion.TryConvert(argument, out value) || argument is VariableTree variable1 && Conversion.TryConvert(variable1.Value, out value);
    }

    private static bool TryGetArguments<T>(object[] arguments, Scope scope, out T arg, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 1 : arguments.Length == 1)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg))
            return true;

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 2 : arguments.Length == 2)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg0)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 3 : arguments.Length == 3)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg0)
            && TryResolveImmediateOrVariable(arguments[1], scope, out arg1)
            && TryResolveImmediateOrVariable(arguments[2], scope, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 4 : arguments.Length == 4)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg0)
            && TryResolveImmediateOrVariable(arguments[1], scope, out arg1)
            && TryResolveImmediateOrVariable(arguments[2], scope, out arg2)
            && TryResolveImmediateOrVariable(arguments[3], scope, out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }

    private static string GetCompileError(int lineIndex, string message) => $"Failed to compile instruction on line {lineIndex}: {message}";
}
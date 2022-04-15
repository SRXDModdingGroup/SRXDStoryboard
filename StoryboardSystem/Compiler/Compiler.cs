using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace StoryboardSystem;

internal static class Compiler {
    public static bool TryCompileFile(string name, string directory, ILogger logger, out StoryboardData result) {
        result = null;
        
        string path = Path.Combine(directory, Path.ChangeExtension(name, ".txt"));

        if (!File.Exists(path)) {
            logger.LogMessage($"No file found for {name}");
            
            return false;
        }
        
        logger.LogMessage($"Attempting to parse {name}");
        
        var watch = Stopwatch.StartNew();

        if (!Parser.TryParseFile(path, logger, out var instructions)) {
            logger.LogWarning($"Failed to parse {name}");

            return false;
        }
        
        watch.Stop();
        logger.LogMessage($"Successfully parsed {name} in {watch.ElapsedMilliseconds}ms");
        logger.LogMessage($"Attempting to compile {name}");
        watch.Restart();

        if (!TryCompile(instructions, logger, out result)) {
            logger.LogWarning($"Failed to compile {name}");

            return false;
        }

        watch.Stop();
        logger.LogMessage($"Successfully compiled {name} in {watch.ElapsedMilliseconds}ms");

        return true;
    }

    private static bool TryCompile(List<Instruction> instructions, ILogger logger, out StoryboardData result) {
        result = null;
        
        var assetBundleReferences = new List<LoadedAssetBundleReference>();
        var assetReferences = new List<LoadedAssetReference>();
        var instanceReferences = new List<LoadedInstanceReference>();
        var postProcessReferences = new List<LoadedPostProcessingReference>();
        var externalObjectReferences = new List<LoadedExternalObjectReference>();
        var timelineBuilders = new List<TimelineBuilder>();
        var outParams = new Dictionary<string, object>();
        var bindings = new Dictionary<Identifier, TimelineBuilder>();
        var procedures = new Dictionary<Name, Procedure>();
        var globals = new Dictionary<Name, object>();
        var globalScope = new Scope(null, 0, 0, 0, Timestamp.Zero, Timestamp.Zero, globals, null);
        bool inProcs = false;

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];
            var opcode = instruction.Opcode;
            
            if (inProcs && opcode != Opcode.Proc)
                continue;
            
            object[] arguments = instruction.Arguments;
            object[] resolvedArguments = new object[arguments.Length];

            for (int j = 0; j < arguments.Length; j++) {
                if (TryResolveArgument(arguments[j], globalScope, logger, out resolvedArguments[j], false))
                    continue;
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {j}"));

                return false;
            }

            if (opcode == Opcode.Proc) {
                if (!TryGetArguments(resolvedArguments, globalScope, logger, out Name name, true)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction Proc"));

                    return false;
                }

                if (procedures.ContainsKey(name)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Procedure {name} already exists"));

                    return false;
                }

                var argNames = new Name[resolvedArguments.Length - 1];

                for (int j = 1, k = 0; j < resolvedArguments.Length; j++, k++) {
                    if (resolvedArguments[j] is not Name argName) {
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

                continue;
            }

            switch (opcode) {
                case Opcode.Bind when TryGetArguments(resolvedArguments, globalScope, logger, out TimelineBuilder timelineBuilder, out Identifier identifier):
                    if (bindings.ContainsKey(identifier)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                        return false;
                    }
                    
                    bindings.Add(identifier, timelineBuilder);

                    break;
                case Opcode.Bundle when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out string bundlePath):
                    var newAssetBundleReference = new LoadedAssetBundleReference(bundlePath);

                    assetBundleReferences.Add(newAssetBundleReference);
                    globals[name] = new Identifier(newAssetBundleReference, Array.Empty<object>());

                    break;
                case Opcode.Curve when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, true): {
                    var timelineBuilder = new TimelineBuilder(name.ToString());
                    
                    timelineBuilders.Add(timelineBuilder);
                    globals[name] = timelineBuilder;
                    
                    for (int j = 1; j < resolvedArguments.Length; j++) {
                        if (resolvedArguments[j] is not Identifier identifier) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, "Invalid arguments for instruction Curve"));

                            return false;
                        }

                        if (bindings.ContainsKey(identifier)) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                            return false;
                        }
                        
                        bindings.Add(identifier, timelineBuilder);
                    }

                    break;
                }
                case Opcode.In when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out string paramName): {
                    var newObjectReference = new LoadedExternalObjectReference(paramName);
                    
                    externalObjectReferences.Add(newObjectReference);
                    globals[name] = new Identifier(newObjectReference, Array.Empty<object>());

                    break;
                }
                case Opcode.Inst when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out LoadedAssetReference assetReference): {
                    var newInstanceReference = assetReference.CreateInstanceReference(name.ToString(), null, 0);

                    instanceReferences.Add(newInstanceReference);
                    globals[name] = new Identifier(newInstanceReference, Array.Empty<object>());

                    break;
                }
                case Opcode.Inst when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out LoadedAssetReference assetReference, out Identifier identifier, out int layer): {
                    var newInstanceReference = assetReference.CreateInstanceReference(name.ToString(), identifier, layer);

                    instanceReferences.Add(newInstanceReference);
                    globals[name] = new Identifier(newInstanceReference, Array.Empty<object>());

                    break;
                }
                case Opcode.Inst when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out LoadedAssetReference assetReference, out Identifier identifier, out string layer): {
                    var newInstanceReference = assetReference.CreateInstanceReference(name.ToString(), identifier, LayerMask.NameToLayer(layer));

                    instanceReferences.Add(newInstanceReference);
                    globals[name] = new Identifier(newInstanceReference, Array.Empty<object>());

                    break;
                }
                case Opcode.InstA when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out int count, out LoadedAssetReference assetReference): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, assetReference, null, 0);

                    break;
                }
                case Opcode.InstA when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out int count, out LoadedAssetReference assetReference, out Identifier identifier, out int layer): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, assetReference, identifier, layer);

                    break;
                }
                case Opcode.InstA when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out int count, out LoadedAssetReference assetReference, out Identifier identifier, out string layer): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, assetReference, identifier, LayerMask.NameToLayer(layer));

                    break;
                }
                case Opcode.Load when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out AssetType type, out LoadedAssetBundleReference assetBundleReference, out string assetName):
                    var newAssetReference = LoadedAssetReference.Create(assetBundleReference, assetName, type);
                    
                    assetReferences.Add(newAssetReference);
                    globals[name] = new Identifier(newAssetReference, Array.Empty<object>());
                    
                    break;
                case Opcode.Out when TryGetArguments(resolvedArguments, globalScope, logger, out string name, out object value):
                    outParams[name] = value;

                    break;
                case Opcode.Post when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out LoadedAssetReference<Material> materialReference, out Identifier targetCameraIdentifier):
                    var newPostProcessingReference = new LoadedPostProcessingReference(materialReference, targetCameraIdentifier);
                    
                    postProcessReferences.Add(newPostProcessingReference);
                    globals[name] = new Identifier(newPostProcessingReference, Array.Empty<object>());
                    
                    break;
                case Opcode.SetA when TryGetArguments(resolvedArguments, globalScope, logger, out Index idx, out object value):
                    idx.Array[idx.index] = value;

                    break;
                case Opcode.SetG when TryGetArguments(resolvedArguments, globalScope, logger, out Name name, out object value):
                    globals[name] = value;

                    break;
                case Opcode.Call:
                case Opcode.Key:
                case Opcode.Loop:
                case Opcode.Set:
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} must be used within a procedure"));

                    return false;
                default:
                    // var builder = new StringBuilder();
                    //
                    // for (int j = 0; j < arguments.Length; j++) {
                    //     object argument = arguments[j];
                    //
                    //     builder.Append(argument.GetType());
                    //
                    //     if (j < arguments.Length - 1)
                    //         builder.Append(", ");
                    // }

                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}"));

                    return false;
            }

            object[] CreateInstanceArray(string name, int count, LoadedAssetReference assetReference, Identifier parentIdentifier, int layer) {
                object[] newArr = new object[count];

                for (int j = 0; j < count; j++) {
                    var newInstanceReference = assetReference.CreateInstanceReference($"{name}_{j}", parentIdentifier, layer);

                    instanceReferences.Add(newInstanceReference);
                    newArr[i] = new Identifier(newInstanceReference, Array.Empty<object>());
                }

                return newArr;
            }
        }

        if (!procedures.TryGetValue(new Name("Main"), out var procedure)) {
            logger.LogWarning(GetCompileError(0, "Procedure Main could not be found"));

            return false;
        }

        int index1 = procedure.StartIndex;
        int orderCounter = 0;
        var currentScope = new Scope(null, index1, 0, 1, Timestamp.Zero, Timestamp.Zero, globals, new Dictionary<Name, object>());

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
            
            var instruction = instructions[index1];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;
            object[] resolvedArguments = new object[arguments.Length];
            
            for (int i = 0; i < arguments.Length; i++) {
                if (TryResolveArgument(arguments[i], currentScope, logger, out resolvedArguments[i], false))
                    continue;
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {i}"));

                return false;
            }
            
            switch (opcode) {
                case Opcode.Bind when TryGetArguments(resolvedArguments, currentScope, logger, out TimelineBuilder timelineBuilder, out Identifier identifier):
                    if (bindings.ContainsKey(identifier)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                        return false;
                    }
                    
                    bindings.Add(identifier, timelineBuilder);

                    break;
                case Opcode.Call when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out Name name, true):
                    if (!TryCallProcedure(time, name, 2, 1, Timestamp.Zero))
                        return false;

                    break;
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out TimelineBuilder timelineBuilder): {
                    timelineBuilder.AddKey(currentScope.GetGlobalTime(time), null, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out TimelineBuilder timelineBuilder, out object value): {
                    timelineBuilder.AddKey(currentScope.GetGlobalTime(time), value, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out TimelineBuilder timelineBuilder, out object value, out InterpType interpType): {
                    timelineBuilder.AddKey(currentScope.GetGlobalTime(time), value, interpType, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out Identifier identifier): {
                    GetImplicitTimelineBuilder(identifier).AddKey(currentScope.GetGlobalTime(time), null, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out Identifier identifier, out object value): {
                    GetImplicitTimelineBuilder(identifier).AddKey(currentScope.GetGlobalTime(time), value, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out Identifier identifier, out object value, out InterpType interpType): {
                    GetImplicitTimelineBuilder(identifier).AddKey(currentScope.GetGlobalTime(time), value, interpType, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Loop when TryGetArguments(resolvedArguments, currentScope, logger, out Timestamp time, out Name name, out int iterations, out Timestamp every, true):
                    if (!TryCallProcedure(time, name, 4, iterations, every))
                        return false;
                    
                    break;
                case Opcode.Set when TryGetArguments(resolvedArguments, currentScope, logger, out Name name, out object value):
                    currentScope.SetValue(name, value);

                    break;
                case Opcode.SetA when TryGetArguments(resolvedArguments, currentScope, logger, out Index idx, out object value): 
                    idx.Array[idx.index] = value;

                    break;
                case Opcode.SetG when TryGetArguments(resolvedArguments, currentScope, logger, out Name name, out object value):
                    globals[name] = value;

                    break;
                case Opcode.Bundle:
                case Opcode.Curve:
                case Opcode.In:
                case Opcode.Inst:
                case Opcode.InstA:
                case Opcode.Load:
                case Opcode.Out:
                case Opcode.Post:
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure"));

                    return false;
                case Opcode.Proc:
                default:
                    // var builder = new StringBuilder();
                    //
                    // for (int i = 0; i < arguments.Length; i++) {
                    //     object argument = arguments[i];
                    //
                    //     builder.Append(argument.GetType());
                    //
                    //     if (i < arguments.Length - 1)
                    //         builder.Append(", ");
                    // }

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

                if (resolvedArguments.Length != argNames.Length + shift) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {name}"));

                    return false;
                }

                int newIndex = procedure.StartIndex;

                if (!currentScope.CheckForRecursion(newIndex)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Recursive procedure call detected"));

                    return false;
                }

                var locals = new Dictionary<Name, object>();

                for (int i = shift, j = 0; i < resolvedArguments.Length; i++, j++) {
                    if (!TryResolveArgument(resolvedArguments[i], currentScope, logger, out object fullyResolvedArgument, true)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {i}"));
                        
                        return false;
                    }
                    
                    locals.Add(argNames[j], fullyResolvedArgument);
                }

                currentScope = new Scope(currentScope, newIndex, index1, iterations, currentScope.GetGlobalTime(time), every, globals, locals);
                index1 = newIndex;
                
                return true;
            }

            TimelineBuilder GetImplicitTimelineBuilder(Identifier identifier) {
                if (bindings.TryGetValue(identifier, out var timelineBuilder))
                    return timelineBuilder;

                timelineBuilder = new TimelineBuilder(identifier.ToString());
                timelineBuilders.Add(timelineBuilder);
                bindings.Add(identifier, timelineBuilder);

                return timelineBuilder;
            }
        }

        foreach (var pair in bindings)
            pair.Value.AddBinding(pair.Key);

        result = new StoryboardData(externalObjectReferences.ToArray(),
            assetBundleReferences.ToArray(),
            assetReferences.ToArray(),
            instanceReferences.ToArray(),
            postProcessReferences.ToArray(),
            timelineBuilders.ToArray(),
            outParams);

        return true;
    }

    private static bool TryResolveArgument(object argument, Scope scope, ILogger logger, out object result, bool resolveFully) {
        result = argument;
        
        switch (result) {
            case object[] arr: {
                object[] newArr = new object[arr.Length];
            
                for (int i = 0; i < arr.Length; i++) {
                    if (!TryResolveArgument(arr[i], scope, logger, out newArr[i], true))
                        return false;
                }

                result = newArr;

                return true;
            }
            case Chain chain: {
                if (chain.Length == 0 || chain[0] is not Name name0) {
                    logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }
                
                result = name0;

                if (!resolveFully && chain.Length == 1)
                    return true;
                
                if (!TryResolveArgument(result, scope, logger, out result, true)) {
                    logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }
                
                object[] sequence = new object[chain.Length - 1];

                for (int i = 1, j = 0; i < chain.Length; i++, j++) {
                    object node = chain[i];

                    switch (node) {
                        case Indexer indexer: {
                            if (!TryResolveArgument(indexer.Token, scope, logger, out object obj0, true) || !TryCastArgument(obj0, scope, logger, out int index0))
                                return false;

                            sequence[j] = index0;
                        
                            continue;
                        }
                        case Name name1:
                            sequence[j] = name1.ToString();
                        
                            continue;
                    }
                    
                    logger.LogWarning($"Chain contains an invalid token");
                
                    return false;
                }

                for (int i = 0; i < sequence.Length; i++) {
                    switch (sequence[i]) {
                        case int index when result is object[] arr: {
                            if (!resolveFully && i == sequence.Length - 1) {
                                result = new Index(arr, index);

                                return true;
                            }

                            result = arr[MathUtility.Mod(index, arr.Length)];

                            continue;
                        }
                        case string:
                        case int: {
                            if (result is not Identifier identifier) {
                                logger.LogWarning($"Chain contains an invalid argument. Type was {result.GetType()}");

                                return false;
                            }

                            object[] oldSequence = identifier.Sequence;
                            object[] newSequence = new object[oldSequence.Length + sequence.Length - i];
                            
                            oldSequence.CopyTo(newSequence, 0);
                            
                            var reference = identifier.Reference;

                            for (int j = oldSequence.Length; i < sequence.Length; i++, j++)
                                newSequence[j] = sequence[i];
                            
                            result = new Identifier(reference, newSequence);

                            return true;
                        }
                    }
                    
                    logger.LogWarning($"Chain contains an invalid token");
                
                    return false;
                }

                return true;
            }
            case FuncCall call: {
                object[] args = call.Arguments;
                object[] resolvedArgs = new object[args.Length];

                for (int i = 0; i < args.Length; i++) {
                    if (!TryResolveArgument(args[i], scope, logger, out resolvedArgs[i], true))
                        return false;
                }

                if (Functions.TryDoFunction(call.Name, resolvedArgs, logger, out result))
                    return true;
                
                logger.LogWarning($"Could not evaluate function call {call.Name}");
                    
                return false;
            }
            default:
                return TryCastArgument(result, scope, logger, out result);
        }
    }

    private static bool TryCastArgument<T>(object argument, Scope scope, ILogger logger, out T value) {
        if (typeof(T) != typeof(object) && argument is T cast0) {
            value = cast0;

            return true;
        }

        switch (argument) {
            case Name name:
                if (scope.TryGetValue(name, out argument))
                    break;

                logger.LogWarning($"Variable {name} not found");
                value = default;
                
                return false;
            case Index index:
                object[] array = index.Array;

                if (array.Length > 0) {
                    argument = array[MathUtility.Mod(index.index, array.Length)];

                    break;
                }

                logger.LogWarning($"Array length can not be 0");
                value = default;
                        
                return false;
        }

        switch (argument) {
            case T cast1:
                value = cast1;

                return true;
            case Identifier identifier when identifier.Sequence.Length == 0 && identifier.Reference is T cast2:
                value = cast2;

                return true;
            default:
                value = default;
            
                return false;
        }
    }

    private static bool TryGetArguments<T>(object[] arguments, Scope scope, ILogger logger, out T arg, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 1 : arguments.Length == 1)
            && TryCastArgument(arguments[0], scope, logger, out arg))
            return true;

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(object[] arguments, Scope scope, ILogger logger, out T0 arg0, out T1 arg1, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 2 : arguments.Length == 2)
            && TryCastArgument(arguments[0], scope, logger, out arg0)
            && TryCastArgument(arguments[1], scope, logger, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(object[] arguments, Scope scope, ILogger logger, out T0 arg0, out T1 arg1, out T2 arg2, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 3 : arguments.Length == 3)
            && TryCastArgument(arguments[0], scope, logger, out arg0)
            && TryCastArgument(arguments[1], scope, logger, out arg1)
            && TryCastArgument(arguments[2], scope, logger, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, Scope scope, ILogger logger, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 4 : arguments.Length == 4)
            && TryCastArgument(arguments[0], scope, logger, out arg0)
            && TryCastArgument(arguments[1], scope, logger, out arg1)
            && TryCastArgument(arguments[2], scope, logger, out arg2)
            && TryCastArgument(arguments[3], scope, logger, out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3, T4>(object[] arguments, Scope scope, ILogger logger, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4, bool unlimited = false) {
        if ((unlimited ? arguments.Length >= 5 : arguments.Length == 5)
            && TryCastArgument(arguments[0], scope, logger, out arg0)
            && TryCastArgument(arguments[1], scope, logger, out arg1)
            && TryCastArgument(arguments[2], scope, logger, out arg2)
            && TryCastArgument(arguments[3], scope, logger, out arg3)
            && TryCastArgument(arguments[4], scope, logger, out arg4))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;
        arg4 = default;

        return false;
    }

    private static string GetCompileError(int lineIndex, string message) => $"Failed to compile instruction on line {lineIndex}: {message}";
}
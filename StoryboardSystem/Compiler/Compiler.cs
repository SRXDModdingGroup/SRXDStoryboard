using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StoryboardSystem;

internal static class Compiler {
    public static bool TryCompileFile(string name, string directory, out StoryboardData result) {
        result = null;

        var logger = StoryboardManager.Instance.Logger;
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

        if (!TryCompile(instructions, out result)) {
            logger.LogWarning($"Failed to compile {name}");

            return false;
        }

        watch.Stop();
        logger.LogMessage($"Successfully compiled {name} in {watch.ElapsedMilliseconds}ms");

        return true;
    }

    private static bool TryCompile(List<Instruction> instructions, out StoryboardData result) {
        result = null;

        var logger = StoryboardManager.Instance.Logger;
        using var resolvedArguments = PooledList.Get();
        var objectReferences = new List<LoadedObjectReference>();
        var timelineBuilders = new List<TimelineBuilder>();
        var outParams = new Dictionary<string, object>();
        var bindings = new Dictionary<Identifier, TimelineBuilder>();
        var procedures = new Dictionary<Name, Procedure>();
        var globals = new Dictionary<Name, object>();
        var globalScope = new Procedure(0, globals, null, null);
        bool inProcs = false;

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];
            var opcode = instruction.Opcode;
            
            if (inProcs && opcode != Opcode.Proc)
                continue;
            
            object[] arguments = instruction.Arguments;
            
            resolvedArguments.Clear();

            for (int j = 0; j < arguments.Length; j++) {
                if (TryResolveArgument(arguments[j], globalScope, out object resolved, false)) {
                    resolvedArguments.Add(resolved);
                    
                    continue;
                }
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {j}"));

                return false;
            }

            int length = arguments.Length;

            if (opcode == Opcode.Proc) {
                if (length < 1 || !TryGetArguments(resolvedArguments, globalScope, out Name name)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for instruction Proc"));

                    return false;
                }

                if (procedures.ContainsKey(name)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Procedure {name} already exists"));

                    return false;
                }

                var argNames = new Name[resolvedArguments.Count - 1];

                for (int j = 1, k = 0; j < resolvedArguments.Count; j++, k++) {
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

                procedures.Add(name, new Procedure(i, globals, new Dictionary<Name, object>(), argNames));
                inProcs = true;

                continue;
            }

            switch (opcode) {
                case Opcode.Bind when length == 2 && TryGetArguments(resolvedArguments, globalScope, out TimelineBuilder timelineBuilder, out IdentifierTree tree): {
                    var identifier = tree.GetIdentifier();

                    if (bindings.ContainsKey(identifier)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                        return false;
                    }

                    bindings.Add(identifier, timelineBuilder);

                    break;
                }
                case Opcode.Bundle when length == 2 && TryGetArguments(resolvedArguments, globalScope, out Name name, out string bundlePath): {
                    AddObjectReference(name, new LoadedAssetBundleReference(bundlePath));

                    break;
                }
                case Opcode.Curve when length >= 1 && TryGetArguments(resolvedArguments, globalScope, out Name name): {
                    var timelineBuilder = new TimelineBuilder(name.ToString());
                    
                    timelineBuilders.Add(timelineBuilder);
                    globals[name] = timelineBuilder;
                    
                    for (int j = 1; j < resolvedArguments.Count; j++) {
                        if (resolvedArguments[j] is not IdentifierTree tree) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, "Invalid arguments for instruction Curve"));

                            return false;
                        }

                        var identifier = tree.GetIdentifier();

                        if (bindings.ContainsKey(identifier)) {
                            logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                            return false;
                        }
                        
                        bindings.Add(identifier, timelineBuilder);
                    }

                    break;
                }
                case Opcode.In when length == 2 && TryGetArguments(resolvedArguments, globalScope, out Name name, out string paramName): {
                    AddObjectReference(name, new LoadedExternalObjectReference(paramName));

                    break;
                }
                case Opcode.Inst when length == 2 && TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), null, 0, string.Empty));

                    break;
                }
                case Opcode.Inst when length == 4 && TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree parent, out int layer): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), layer, string.Empty));

                    break;
                }
                case Opcode.Inst when length == 4 && TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree parent, out string layer): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), 0, layer));

                    break;
                }
                case Opcode.InstA when length == 3 && TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template): {
                    globals[name] = CreateInstanceArray(count, template, null, 0, string.Empty);

                    break;
                }
                case Opcode.InstA when length == 5 && TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template, out IdentifierTree parent, out int layer): {
                    globals[name] = CreateInstanceArray(count, template, parent, layer, string.Empty);

                    break;
                }
                case Opcode.InstA when length == 5 && TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template, out IdentifierTree parent, out string layer): {
                    globals[name] = CreateInstanceArray(count, template, parent, 0, layer);

                    break;
                }
                case Opcode.Load when length == 3 && TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree assetBundle, out string assetName): {
                    AddObjectReference(name, new LoadedAssetReference(assetBundle.GetIdentifier(), assetName));

                    break;
                }
                case Opcode.Out when length == 2 && TryGetArguments(resolvedArguments, globalScope, out string name, out object value): {
                    outParams[name] = value;

                    break;
                }
                case Opcode.Post when length == 3 && TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree camera): {
                    AddObjectReference(name, new LoadedPostProcessingReference(template.GetIdentifier(), camera.GetIdentifier()));

                    break;
                }
                case Opcode.SetA when length == 2 && TryGetArguments(resolvedArguments, globalScope, out Index idx, out object value): {
                    idx.Array[idx.index] = value;

                    break;
                }
                case Opcode.SetG when length == 2 && TryGetArguments(resolvedArguments, globalScope, out Name name, out object value): {
                    globals[name] = value;

                    break;
                }
                case Opcode.Call:
                case Opcode.Key:
                case Opcode.Loop:
                case Opcode.Set: {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} must be used within a procedure"));

                    return false;
                }
                default: {
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
            }

            void AddObjectReference(Name name, LoadedObjectReference reference) {
                globals[name] = new IdentifierTree(objectReferences.Count);
                objectReferences.Add(reference);
            }

            object[] CreateInstanceArray(int count, IdentifierTree template, IdentifierTree parent, int layer, string layerS) {
                object[] newArr = new object[count];

                for (int j = 0; j < count; j++) {
                    newArr[i] = new IdentifierTree(objectReferences.Count);
                    objectReferences.Add(new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), layer, layerS));
                }

                return newArr;
            }
        }

        if (!procedures.TryGetValue(new Name("Main"), out var currentProcedure)) {
            logger.LogWarning(GetCompileError(0, "Procedure Main could not be found"));

            return false;
        }
        
        currentProcedure.Init(0, null, 1, Timestamp.Zero, Timestamp.Zero);

        int index = currentProcedure.StartIndex;
        int orderCounter = 0;

        while (currentProcedure != null) {
            index++;
            
            if (index >= instructions.Count || instructions[index].Opcode == Opcode.Proc) {
                if (currentProcedure.NextIteration())
                    index = currentProcedure.StartIndex;
                else {
                    index = currentProcedure.ReturnIndex;
                    currentProcedure = currentProcedure.Parent;
                }

                continue;
            }
            
            var instruction = instructions[index];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;
            
            resolvedArguments.Clear();
            
            for (int i = 0; i < arguments.Length; i++) {
                if (TryResolveArgument(arguments[i], currentProcedure, out object resolved, false)) {
                    resolvedArguments.Add(resolved);
                        
                    continue;
                }
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {i}"));

                return false;
            }

            int length = arguments.Length;
            
            switch (opcode) {
                case Opcode.Bind when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out TimelineBuilder timelineBuilder, out IdentifierTree tree): {
                    var identifier = tree.GetIdentifier();

                    if (bindings.ContainsKey(identifier)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple curves"));

                        return false;
                    }

                    bindings.Add(identifier, timelineBuilder);

                    break;
                }
                case Opcode.Call when length >= 2 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out Name name): {
                    if (!TryCallProcedure(time, name, 2, 1, Timestamp.Zero))
                        return false;

                    break;
                }
                case Opcode.Key when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder timelineBuilder): {
                    timelineBuilder.AddKey(currentProcedure.GetGlobalTime(time), null, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when length == 3 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder timelineBuilder, out object value): {
                    timelineBuilder.AddKey(currentProcedure.GetGlobalTime(time), value, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when length == 4 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder timelineBuilder, out object value, out InterpType interpType): {
                    timelineBuilder.AddKey(currentProcedure.GetGlobalTime(time), value, interpType, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), null, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when length == 3 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree, out object value): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), value, InterpType.Fixed, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Key when length == 4 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree, out object value, out InterpType interpType): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), value, interpType, orderCounter);
                    orderCounter++;

                    break;
                }
                case Opcode.Loop when length >= 4 && TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out Name name, out int iterations, out Timestamp every): {
                    if (!TryCallProcedure(time, name, 4, iterations, every))
                        return false;

                    break;
                }
                case Opcode.Set when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out Name name, out object value): {
                    currentProcedure.SetValue(name, value);

                    break;
                }
                case Opcode.SetA when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out Index idx, out object value): {
                    idx.Array[idx.index] = value;

                    break;
                }
                case Opcode.SetG when length == 2 && TryGetArguments(resolvedArguments, currentProcedure, out Name name, out object value): {
                    globals[name] = value;

                    break;
                }
                case Opcode.Bundle:
                case Opcode.Curve:
                case Opcode.In:
                case Opcode.Inst:
                case Opcode.InstA:
                case Opcode.Load:
                case Opcode.Out:
                case Opcode.Post: {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure"));

                    return false;
                }
                case Opcode.Proc:
                default: {
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
            }

            bool TryCallProcedure(Timestamp time, Name name, int shift, int iterations, Timestamp every) {
                if (iterations <= 0) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Iterations must be greater than 0"));

                    return false;
                }
                
                if (!procedures.TryGetValue(name, out var newProcedure)) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Procedure {name} could not be found"));

                    return false;
                }

                var argNames = newProcedure.ArgNames;

                if (resolvedArguments.Count != argNames.Length + shift) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {name}"));

                    return false;
                }

                int newIndex = newProcedure.StartIndex;

                if (!newProcedure.CheckForRecursion()) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Recursive procedure call detected"));

                    return false;
                }

                newProcedure.Init(index, currentProcedure, iterations, currentProcedure.GetGlobalTime(time), every);

                for (int i = shift, j = 0; i < resolvedArguments.Count; i++, j++) {
                    if (!TryCastArgument(resolvedArguments[i], currentProcedure, out object fullyResolvedArgument)) {
                        logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {i}"));
                        
                        return false;
                    }
                    
                    newProcedure.SetValue(argNames[j], fullyResolvedArgument);
                }
                
                index = newIndex;
                currentProcedure = newProcedure;
                
                return true;
            }

            TimelineBuilder GetImplicitTimelineBuilder(IdentifierTree tree) {
                var identifier = tree.GetIdentifier();
                
                if (bindings.TryGetValue(identifier, out var timelineBuilder))
                    return timelineBuilder;

                timelineBuilder = new TimelineBuilder(identifier.ToString());
                timelineBuilders.Add(timelineBuilder);
                bindings.Add(identifier, timelineBuilder);

                return timelineBuilder;
            }
        }

        foreach (var pair in bindings)
            pair.Value.AddIdentifier(pair.Key);

        result = new StoryboardData(objectReferences, timelineBuilders, outParams);

        return true;
    }

    private static bool TryResolveArgument(object argument, Procedure procedure, out object result, bool resolveFully) {
        result = argument;
        
        switch (result) {
            case object[] arr: {
                object[] newArr = new object[arr.Length];
                
                for (int i = 0; i < arr.Length; i++) {
                    if (!TryResolveArgument(arr[i], procedure, out newArr[i], true))
                        return false;
                }

                result = newArr;

                return true;
            }
            case Chain chain: {
                if (chain.Length == 0 || chain[0] is not Name name0) {
                    StoryboardManager.Instance.Logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }
                
                result = name0;

                if (!resolveFully && chain.Length == 1)
                    return true;
                
                if (!TryResolveArgument(result, procedure, out result, true)) {
                    StoryboardManager.Instance.Logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }

                for (int i = 1; i < chain.Length; i++) {
                    object node = chain[i];

                    switch (node) {
                        case Indexer indexer: {
                            if (!TryResolveArgument(indexer.Token, procedure, out object obj0, true) || !TryCastArgument(obj0, procedure, out int index))
                                return false;

                            switch (result) {
                                case object[] arr:
                                    result = arr[index];
                                    break;
                                case IdentifierTree identifier0:
                                    result = identifier0.GetChild(index);
                                    break;
                                default:
                                    StoryboardManager.Instance.Logger.LogWarning("Chain contains an invalid argument");

                                    return false;
                            }
                        
                            continue;
                        }
                        case Name name1:
                            if (result is not IdentifierTree identifier1) {
                                StoryboardManager.Instance.Logger.LogWarning("Chain contains an invalid argument");

                                return false;
                            }
                            
                            result = identifier1.GetChild(name1.ToString());

                            continue;
                        default:
                            StoryboardManager.Instance.Logger.LogWarning("Chain contains an invalid token");
                
                            return false;
                    }
                }

                return true;
            }
            case FuncCall call: {
                using var resolvedArgs = PooledList.Get();
                object[] args = call.Arguments;

                foreach (object arg in args) {
                    if (!TryResolveArgument(arg, procedure, out object resolved, true))
                        return false;
                    
                    resolvedArgs.Add(resolved);
                }

                if (Functions.TryDoFunction(call.Name, resolvedArgs, out result))
                    return true;
                
                StoryboardManager.Instance.Logger.LogWarning($"Could not evaluate function call {call.Name}");
                    
                return false;
            }
            default:
                return TryCastArgument(result, procedure, out result);
        }
    }

    private static bool TryCastArgument<T>(object argument, Procedure procedure, out T value) {
        if (typeof(T) != typeof(object) && argument is T cast0) {
            value = cast0;

            return true;
        }

        switch (argument) {
            case Name name:
                if (procedure.TryGetValue(name, out argument))
                    break;

                StoryboardManager.Instance.Logger.LogWarning($"Variable {name} not found");
                value = default;
                
                return false;
            case Index index:
                object[] array = index.Array;

                if (array.Length > 0) {
                    argument = array[MathUtility.Mod(index.index, array.Length)];

                    break;
                }

                StoryboardManager.Instance.Logger.LogWarning($"Array length can not be 0");
                value = default;
                        
                return false;
        }

        if (argument is T cast1) {
            value = cast1;

            return true;
        }

        value = default;

        return false;
    }

    private static bool TryGetArguments<T>(List<object> arguments, Procedure procedure, out T arg) {
        if (TryCastArgument(arguments[0], procedure, out arg))
            return true;

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1) {
        if (TryCastArgument(arguments[0], procedure, out arg0)
            && TryCastArgument(arguments[1], procedure, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1, out T2 arg2) {
        if (TryCastArgument(arguments[0], procedure, out arg0)
            && TryCastArgument(arguments[1], procedure, out arg1)
            && TryCastArgument(arguments[2], procedure, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3) {
        if (TryCastArgument(arguments[0], procedure, out arg0)
            && TryCastArgument(arguments[1], procedure, out arg1)
            && TryCastArgument(arguments[2], procedure, out arg2)
            && TryCastArgument(arguments[3], procedure, out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3, T4>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4) {
        if (TryCastArgument(arguments[0], procedure, out arg0)
            && TryCastArgument(arguments[1], procedure, out arg1)
            && TryCastArgument(arguments[2], procedure, out arg2)
            && TryCastArgument(arguments[3], procedure, out arg3)
            && TryCastArgument(arguments[4], procedure, out arg4))
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
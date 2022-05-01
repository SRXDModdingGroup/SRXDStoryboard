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

        if (!Parser.TryParseFile(path, out var instructions)) {
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
        var implicitTimelineBuilders = new Dictionary<Identifier, TimelineBuilder>();
        var outParams = new Dictionary<string, object>();
        var bindings = new Dictionary<Identifier, Identifier>();
        var globals = new Dictionary<Name, object>();
        var globalScope = new Procedure("Global", 0, globals, null, null);
        bool inProcs = false;

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];
            var opcode = instruction.Opcode;
            
            if (inProcs && opcode != Opcode.Proc)
                continue;
            
            var arguments = instruction.Arguments;
            
            resolvedArguments.Clear();

            for (int j = 0; j < arguments.Length; j++) {
                if (TryResolveArgument(arguments[j], globalScope, out object resolved)) {
                    resolvedArguments.Add(resolved);
                    
                    continue;
                }
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {j}"));

                return false;
            }

            int length = arguments.Length;

            switch (opcode, length) {
                case (Opcode.Bind, 2) when TryGetArguments(resolvedArguments, globalScope, out IdentifierTree controllerTree, out IdentifierTree propertyTree): {
                    var controller = controllerTree.GetIdentifier();
                    var property = propertyTree.GetIdentifier();

                    if (bindings.ContainsKey(property)) {
                        StoryboardManager.Instance.Logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple controllers"));

                        return false;
                    }

                    bindings.Add(property, controller);

                    continue;
                }
                case (Opcode.Bind, 2) when TryGetArguments(resolvedArguments, globalScope, out TimelineBuilder builder, out IdentifierTree propertyTree): {
                    var controller = builder.Identifier;
                    var property = propertyTree.GetIdentifier();

                    if (bindings.ContainsKey(property)) {
                        StoryboardManager.Instance.Logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple controllers"));

                        return false;
                    }

                    bindings.Add(property, controller);

                    continue;
                }
                case (Opcode.Bundle, 2) when TryGetArguments(resolvedArguments, globalScope, out Name name, out string bundlePath): {
                    AddObjectReference(name, new LoadedAssetBundleReference(bundlePath));

                    continue;
                }
                case (Opcode.Curve, 1) when TryGetArguments(resolvedArguments, out Name name): {
                    var controller = new IdentifierTree(name.ToString(), objectReferences.Count).GetIdentifier();
                    var timelineBuilder = new TimelineBuilder(name.ToString(), controller);
                    
                    globals[name] = timelineBuilder;
                    objectReferences.Add(new LoadedTimelineReference(controller, timelineBuilder));

                    continue;
                }
                case (Opcode.In, 2) when TryGetArguments(resolvedArguments, globalScope, out Name name, out string paramName): {
                    AddObjectReference(name, new LoadedExternalObjectReference(paramName));

                    continue;
                }
                case (Opcode.Inst, 2) when TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), null, 0, string.Empty));

                    continue;
                }
                case (Opcode.Inst, 4) when TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree parent, out int layer): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), layer, string.Empty));

                    continue;
                }
                case (Opcode.Inst, 4) when TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree parent, out string layer): {
                    AddObjectReference(name, new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), 0, layer));

                    continue;
                }
                case (Opcode.InstA, 3) when TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, template, null, 0, string.Empty);

                    continue;
                }
                case (Opcode.InstA, 5) when TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template, out IdentifierTree parent, out int layer): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, template, parent, layer, string.Empty);

                    continue;
                }
                case (Opcode.InstA, 5) when TryGetArguments(resolvedArguments, globalScope, out Name name, out int count, out IdentifierTree template, out IdentifierTree parent, out string layer): {
                    globals[name] = CreateInstanceArray(name.ToString(), count, template, parent, 0, layer);

                    continue;
                }
                case (Opcode.Load, 3) when TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree assetBundle, out string assetName): {
                    AddObjectReference(name, new LoadedAssetReference(assetBundle.GetIdentifier(), assetName));

                    continue;
                }
                case (Opcode.Out, 2) when TryGetArguments(resolvedArguments, globalScope, out string name, out object value): {
                    outParams[name] = value;

                    continue;
                }
                case (Opcode.Post, 3) when TryGetArguments(resolvedArguments, globalScope, out Name name, out IdentifierTree template, out IdentifierTree camera): {
                    AddObjectReference(name, new LoadedPostProcessingReference(template.GetIdentifier(), camera.GetIdentifier()));

                    continue;
                }
                case (Opcode.Proc, >= 1) when TryGetArguments(resolvedArguments, out Name name): {
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

                    globals[name] = new Procedure(name.ToString(), i, globals, new Dictionary<Name, object>(), argNames);
                    inProcs = true;

                    continue;
                }
                case (Opcode.SetA, 2) when TryGetArguments(resolvedArguments, globalScope, out Index idx, out object value): {
                    if (idx.TrySetArrayValue(value))
                        continue;

                    return false;
                }
                case (Opcode.SetG, 2) when TryGetArguments(resolvedArguments, globalScope, out Name name, out object value): {
                    globals[name] = value;

                    continue;
                }
                case (Opcode.Call, _):
                case (Opcode.Key, _):
                case (Opcode.Loop, _):
                case (Opcode.Set, _): {
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
                globals[name] = new IdentifierTree(name.ToString(), objectReferences.Count);
                objectReferences.Add(reference);
            }

            object[] CreateInstanceArray(string name, int count, IdentifierTree template, IdentifierTree parent, int layer, string layerS) {
                object[] newArr = new object[count];

                for (int j = 0; j < count; j++) {
                    newArr[i] = new IdentifierTree($"{name}_{j}", objectReferences.Count);
                    objectReferences.Add(new LoadedInstanceReference(template.GetIdentifier(), parent.GetIdentifier(), layer, layerS));
                }

                return newArr;
            }
        }

        if (!globals.TryGetValue(new Name("Main"), out object obj) || obj is not Procedure currentProcedure) {
            logger.LogWarning(GetCompileError(0, "Procedure Main could not be found"));

            return false;
        }
        
        currentProcedure.Init(0, null, 1, Timestamp.Zero, Timestamp.Zero);

        int index = currentProcedure.StartIndex;

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
            var arguments = instruction.Arguments;
            
            resolvedArguments.Clear();
            
            for (int i = 0; i < arguments.Length; i++) {
                if (TryResolveArgument(arguments[i], currentProcedure, out object resolved)) {
                    resolvedArguments.Add(resolved);
                        
                    continue;
                }
                
                logger.LogWarning(GetCompileError(instruction.LineIndex, $"Could not resolve argument {i}"));

                return false;
            }

            int length = arguments.Length;
            
            switch (opcode, length) {
                case (Opcode.Bind, 2) when TryGetArguments(resolvedArguments, currentProcedure, out IdentifierTree controllerTree, out IdentifierTree propertyTree): {
                    var controller = controllerTree.GetIdentifier();
                    var property = propertyTree.GetIdentifier();

                    if (bindings.ContainsKey(property)) {
                        StoryboardManager.Instance.Logger.LogWarning(GetCompileError(instruction.LineIndex, "A property can not be bound to multiple controllers"));

                        return false;
                    }

                    bindings.Add(property, controller);

                    continue;
                }
                case (Opcode.Call, >= 2) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out Procedure procedure): {
                    if (!TryCallProcedure(time, procedure, 2, 1, Timestamp.Zero))
                        return false;

                    continue;
                }
                case (Opcode.Key, 2) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), null, InterpType.Fixed);

                    continue;
                }
                case (Opcode.Key, 2) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder builder): {
                    builder.AddKey(currentProcedure.GetGlobalTime(time), null, InterpType.Fixed);

                    continue;
                }
                case (Opcode.Key, 3) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree, out object value): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), value, InterpType.Fixed);

                    continue;
                }
                case (Opcode.Key, 3) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder builder, out object value): {
                    builder.AddKey(currentProcedure.GetGlobalTime(time), value, InterpType.Fixed);

                    continue;
                }
                case (Opcode.Key, 4) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out IdentifierTree tree, out object value, out InterpType interpType): {
                    GetImplicitTimelineBuilder(tree).AddKey(currentProcedure.GetGlobalTime(time), value, interpType);

                    continue;
                }
                case (Opcode.Key, 4) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out TimelineBuilder builder, out object value, out InterpType interpType): {
                    builder.AddKey(currentProcedure.GetGlobalTime(time), value, interpType);

                    continue;
                }
                case (Opcode.Loop, >= 4) when TryGetArguments(resolvedArguments, currentProcedure, out Timestamp time, out Procedure procedure, out int iterations, out Timestamp every): {
                    if (!TryCallProcedure(time, procedure, 4, iterations, every))
                        return false;

                    continue;
                }
                case (Opcode.Set, 2) when TryGetArguments(resolvedArguments, currentProcedure, out Name name, out object value): {
                    currentProcedure.SetValue(name, value);

                    continue;
                }
                case (Opcode.SetA, 2) when TryGetArguments(resolvedArguments, currentProcedure, out Index idx, out object value): {
                    if (idx.TrySetArrayValue(value))
                        continue;

                    return false;
                }
                case (Opcode.SetG, 2) when TryGetArguments(resolvedArguments, currentProcedure, out Name name, out object value): {
                    globals[name] = value;

                    continue;
                }
                case (Opcode.Bundle, _):
                case (Opcode.Curve, _):
                case (Opcode.In, _):
                case (Opcode.Inst, _):
                case (Opcode.InstA, _):
                case (Opcode.Load, _):
                case (Opcode.Out, _):
                case (Opcode.Post, _): {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure"));

                    return false;
                }
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

            bool TryCallProcedure(Timestamp time, Procedure newProcedure, int shift, int iterations, Timestamp every) {
                if (iterations <= 0) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Iterations must be greater than 0"));

                    return false;
                }

                var argNames = newProcedure.ArgNames;

                if (resolvedArguments.Count != argNames.Length + shift) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {newProcedure.Name}"));

                    return false;
                }

                int newIndex = newProcedure.StartIndex;

                if (!newProcedure.CheckForRecursion()) {
                    logger.LogWarning(GetCompileError(instruction.LineIndex, "Recursive procedure call detected"));

                    return false;
                }

                newProcedure.Init(index, currentProcedure, iterations, currentProcedure.GetGlobalTime(time), every);

                for (int i = shift, j = 0; i < resolvedArguments.Count; i++, j++) {
                    if (!TryResolveObject(resolvedArguments[i], currentProcedure, out object fullyResolvedArgument)) {
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
                var property = tree.GetIdentifier();

                if (implicitTimelineBuilders.TryGetValue(property, out var builder))
                    return builder;

                string name = property.ToString();
                var controller = new Identifier(name, objectReferences.Count, Array.Empty<object>());

                builder = new TimelineBuilder(name, controller);
                implicitTimelineBuilders.Add(property, builder);
                bindings.Add(property, controller);
                objectReferences.Add(new LoadedTimelineReference(controller, builder));

                return builder;
            }
        }

        var bindingsCToP = new Dictionary<Identifier, List<Identifier>>();

        foreach (var pair in bindings) {
            var controller = pair.Value;

            if (!bindingsCToP.TryGetValue(controller, out var properties)) {
                properties = new List<Identifier>();
                bindingsCToP.Add(controller, properties);
            }
            
            properties.Add(pair.Key);
        }

        result = new StoryboardData(objectReferences, bindingsCToP, outParams);

        return true;
    }

    private static bool TryResolveArgument(Token argument, Procedure procedure, out object result) {
        switch (argument.Type) {
            case TokenType.Array: {
                var arrayT = (ArrayT) argument;
                
                object[] newArr = new object[arrayT.Length];
                
                for (int i = 0; i < arrayT.Length; i++) {
                    if (TryResolveArgument(arrayT[i], procedure, out object obj) && TryResolveObject(obj, procedure, out newArr[i]))
                        continue;
                    
                    result = null;
                        
                    return false;
                }

                result = newArr;

                return true;
            }
            case TokenType.Chain: {
                var chain = (Chain) argument;
                
                if (chain.Length == 0 || chain[0] is not Name name0) {
                    result = null;
                    StoryboardManager.Instance.Logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }
                
                if (chain.Length == 1) {
                    result = name0;
                    
                    return true;
                }
                
                if (!TryResolveObject(name0, procedure, out result)) {
                    StoryboardManager.Instance.Logger.LogWarning("Could not resolve start of chain");
                    
                    return false;
                }

                for (int i = 1; i < chain.Length; i++) {
                    var node = chain[i];

                    switch (node) {
                        case Name name1: {
                            if (result is not IdentifierTree identifier1) {
                                StoryboardManager.Instance.Logger.LogWarning("Chain contains an invalid argument");

                                return false;
                            }

                            result = identifier1.GetChild(name1.ToString());

                            continue;
                        }
                        case Indexer indexer: {
                            if (!TryResolveArgument(indexer.Token, procedure, out object obj) || !TryCastObject(obj, procedure, out int index))
                                return false;

                            switch (result) {
                                case object[] arr:
                                    if (i == chain.Length - 1)
                                        result = new Index(arr, index);
                                    else
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
                        default: {
                            StoryboardManager.Instance.Logger.LogWarning("Chain contains an invalid token");

                            return false;
                        }
                    }
                }

                return true;
            }
            case TokenType.Constant: {
                result = ((Constant) argument).Value;

                return true;
            }
            case TokenType.FuncCall: {
                var call = (FuncCall) argument;
                using var resolvedArgs = PooledList.Get();
                var args = call.Arguments;

                foreach (var arg in args) {
                    if (!TryResolveArgument(arg, procedure, out object obj) || !TryResolveObject(obj, procedure, out object resolved)) {
                        result = null;
                        
                        return false;
                    }
                    
                    resolvedArgs.Add(resolved);
                }

                if (Functions.TryDoFunction(call.Name, resolvedArgs, out result))
                    return true;
                
                StoryboardManager.Instance.Logger.LogWarning($"Could not evaluate function call {call.Name}");
                    
                return false;
            }
            case TokenType.Invalid:
            case TokenType.Indexer:
            case TokenType.Name:
            case TokenType.Opcode:
            default:
                result = null;
                StoryboardManager.Instance.Logger.LogWarning($"Not a valid token");

                return false;
        }
    }

    private static bool TryResolveObject(object obj, Procedure procedure, out object value) {
        switch (obj) {
            case Name name:
                if (procedure.TryGetValue(name, out value))
                    return true;

                StoryboardManager.Instance.Logger.LogWarning($"Variable {name} not found");
                value = null;
                
                return false;
            case Index index:
                return index.TryResolve(out value);
            default:
                value = obj;

                return true;
        }
    }

    private static bool TryCastObject<T>(object obj, Procedure procedure, out T value) {
        if (TryResolveObject(obj, procedure, out obj) && obj is T cast1) {
            value = cast1;

            return true;
        }

        value = default;
            
        return false;
    }

    private static bool TryGetArguments<T0, T1>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1) {
        if (TryCastObject(arguments[0], procedure, out arg0)
            && TryCastObject(arguments[1], procedure, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1, out T2 arg2) {
        if (TryCastObject(arguments[0], procedure, out arg0)
            && TryCastObject(arguments[1], procedure, out arg1)
            && TryCastObject(arguments[2], procedure, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(List<object> arguments, Procedure procedure, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3) {
        if (TryCastObject(arguments[0], procedure, out arg0)
            && TryCastObject(arguments[1], procedure, out arg1)
            && TryCastObject(arguments[2], procedure, out arg2)
            && TryCastObject(arguments[3], procedure, out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }
    
    private static bool TryGetArguments(List<object> arguments, out Name arg) {
        if (arguments[0] is Name name) {
            arg = name;
            
            return true;
        }

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0>(List<object> arguments, Procedure procedure, out Name arg0, out T0 arg1) {
        if (arguments[0] is Name name
            && TryCastObject(arguments[1], procedure, out arg1)) {
            arg0 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(List<object> arguments, Procedure procedure, out Name arg0, out T0 arg1, out T1 arg2) {
        if (arguments[0] is Name name
            && TryCastObject(arguments[1], procedure, out arg1)
            && TryCastObject(arguments[2], procedure, out arg2)) {
            arg0 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(List<object> arguments, Procedure procedure, out Name arg0, out T0 arg1, out T1 arg2, out T2 arg3) {
        if (arguments[0] is Name name
            && TryCastObject(arguments[1], procedure, out arg1)
            && TryCastObject(arguments[2], procedure, out arg2)
            && TryCastObject(arguments[3], procedure, out arg3)) {
            arg0 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(List<object> arguments, Procedure procedure, out Name arg0, out T0 arg1, out T1 arg2, out T2 arg3, out T3 arg4) {
        if (arguments[0] is Name name
            && TryCastObject(arguments[1], procedure, out arg1)
            && TryCastObject(arguments[2], procedure, out arg2)
            && TryCastObject(arguments[3], procedure, out arg3)
            && TryCastObject(arguments[4], procedure, out arg4)) {
            arg0 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;
        arg4 = default;

        return false;
    }
    
    private static bool TryGetArguments<T0>(List<object> arguments, Procedure procedure, out T0 arg0, out Name arg1) {
        if (arguments[1] is Name name
            && TryCastObject(arguments[0], procedure, out arg0)) {
            arg1 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(List<object> arguments, Procedure procedure, out T0 arg0, out Name arg1, out T1 arg2) {
        if (arguments[1] is Name name
            && TryCastObject(arguments[0], procedure, out arg0)
            && TryCastObject(arguments[2], procedure, out arg2)) {
            arg1 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(List<object> arguments, Procedure procedure, out T0 arg0, out Name arg1, out T1 arg2, out T2 arg3) {
        if (arguments[1] is Name name
            && TryCastObject(arguments[0], procedure, out arg0)
            && TryCastObject(arguments[2], procedure, out arg2)
            && TryCastObject(arguments[3], procedure, out arg3)) {
            arg1 = name;
            
            return true;
        }

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }

    private static bool TryGetArguments<T0>(List<object> arguments, Procedure procedure, out Index arg0, out T0 arg1) {
        if (arguments[0] is Index index
            && TryCastObject(arguments[1], procedure, out arg1)) {
            arg0 = index;
            
            return true;
        }

        arg0 = default;
        arg1 = default;

        return false;
    }


    private static string GetCompileError(int lineIndex, string message) => $"Failed to compile instruction on line {lineIndex}: {message}";
}
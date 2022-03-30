using System.Collections.Generic;

namespace SRXDStoryboard.Plugin;

public static class Compiler {
    public static bool TryCompileFile(string path, out Storyboard storyboard) {
        if (Parser.TryParseFile(path, out var instructions))
            return TryCompile(instructions, out storyboard);
        
        storyboard = null;
            
        return false;
    }

    private static bool TryCompile(List<Instruction> instructions, out Storyboard storyboard) {
        storyboard = null;
        
        var procedures = new Dictionary<string, Procedure>();

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;

            switch (opcode) {
                case Opcode.Inst:
                    break;
                case Opcode.Load:
                    break;
                case Opcode.Proc:
                    if (arguments.Length < 1 || arguments[0] is not string[] { Length: 1 } str0) {
                        ThrowCompileError(instruction.LineIndex, "Invalid arguments for instruction Proc");

                        return false;
                    }

                    string name = str0[0];
                    
                    if (procedures.ContainsKey(name)) {
                        ThrowCompileError(instruction.LineIndex, $"Procedure {name} already exists");
                
                        return false;
                    }
                    
                    string[] argNames = new string[arguments.Length - 1];

                    for (int j = 1, k = 0; j < arguments.Length; j++, k++) {
                        if (arguments[j] is not string[] { Length: 1 } str1) {
                            ThrowCompileError(instruction.LineIndex, "Invalid arguments for instruction Proc");

                            return false;
                        }

                        string argName = str1[0];

                        if (argNames.Contains(argName)) {
                            ThrowCompileError(instruction.LineIndex, $"Argument name {argName} already exists");

                            return false;
                        }

                        argNames[k] = argName;
                    }

                    procedures.Add(name, new Procedure(i, argNames));
                    
                    break;
                case Opcode.Call:
                case Opcode.Event:
                case Opcode.Key:
                case Opcode.Loop:
                case Opcode.Set:
                case Opcode.SetG:
                    break;
                default:
                    ThrowCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}");

                    return false;
            }
        }

        if (!procedures.TryGetValue("Main", out var procedure)) {
            ThrowCompileError(0, "Procedure Main could not be found");

            return false;
        }

        int index = procedure.StartIndex;
        var globals = new Dictionary<string, object>();
        var currentScope = new Scope(null, Timestamp.Zero, index, 0, 1, globals, new Dictionary<string, object>());

        while (currentScope != null) {
            index++;
            
            if (index >= instructions.Count || instructions[index].Opcode == Opcode.Proc) {
                if (currentScope.NextIteration())
                    index = currentScope.StartIndex;
                else {
                    index = currentScope.ReturnIndex;
                    currentScope = currentScope.Parent;
                }

                continue;
            }
            
            var instruction = instructions[index];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;
            
            switch (opcode) {
                case Opcode.Call:
                    if (!TryCallProcedure(false))
                        return false;

                    break;
                case Opcode.Event:
                    break;
                case Opcode.Key:
                    break;
                case Opcode.Loop:
                    if (!TryCallProcedure(true))
                        return false;
                    
                    break;
                case Opcode.Set when TryGetArguments(arguments, currentScope, out string[] str, out object value) && str.Length == 1:
                    currentScope.SetValue(str[0], value);

                    break;
                case Opcode.SetG when TryGetArguments(arguments, currentScope, out string[] str, out object value) && str.Length == 1:
                    globals[str[0]] = value;

                    break;
                case Opcode.Inst:
                case Opcode.Load:
                    ThrowCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure");

                    return false;
                case Opcode.Proc:
                default:
                    ThrowCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}");

                    return false;
            }

            bool TryCallProcedure(bool isLoop) {
                if (arguments.Length < 1 || arguments[0] is not string[] { Length: 1 } str0) {
                    ThrowCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}");

                    return false;
                }

                string name = str0[0];

                if (!procedures.TryGetValue(name, out procedure)) {
                    ThrowCompileError(instruction.LineIndex, $"Procedure {name} could not be found");

                    return false;
                }

                int iterations;

                if (isLoop) {
                    if (arguments.Length < 2 || arguments[1] is not int intVal) {
                        ThrowCompileError(instruction.LineIndex, $"Invalid arguments for instruction Loop");

                        return false;
                    }

                    iterations = intVal;
                }
                else
                    iterations = 1;

                int shift = isLoop ? 2 : 1;
                string[] argNames = procedure.ArgNames;

                if (arguments.Length != argNames.Length + shift) {
                    ThrowCompileError(instruction.LineIndex, $"Invalid arguments for procedure call {name}");

                    return false;
                }

                int newIndex = procedure.StartIndex;

                if (!currentScope.CheckForRecursion(newIndex)) {
                    ThrowCompileError(instruction.LineIndex, "Recursive procedure call detected");

                    return false;
                }

                var locals = new Dictionary<string, object>();

                for (int i = shift, j = 0; i < arguments.Length; i++, j++)
                    locals.Add(argNames[j], arguments[i]);

                currentScope = new Scope(currentScope, currentScope.StartTime + instruction.Timestamp, newIndex, index, iterations, globals, locals);
                index = newIndex;
                
                return true;
            }
        }

        return true;
    }
    
    
    
    private static void ThrowCompileError(int lineIndex, string message)
        => Plugin.Logger.LogWarning($"Failed to compile instruction on line {lineIndex}: {message}");

    private static bool TryResolveImmediateOrVariable<T>(object argument, Scope scope, out T value) {
        value = default;
        
        if (argument is string[] hierarchy && scope != null) {
            if (!scope.TryGetValue(hierarchy[0], out argument))
                return false;

            for (int i = 1; i < hierarchy.Length; i++) {
                if (argument is not Variable variable0 || !variable0.TryGetSubVariable(hierarchy[i], out argument))
                    return false;
            }
        }

        return Conversion.TryConvert(argument, out value) || argument is Variable variable1 && Conversion.TryConvert(variable1.Value, out value);
    }

    private static bool TryGetArguments(object[] arguments, Scope scope) => arguments.Length == 0;
    private static bool TryGetArguments<T>(object[] arguments, Scope scope, out T arg) {
        if (arguments.Length == 1 && TryResolveImmediateOrVariable(arguments[0], scope, out arg))
            return true;

        arg = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1) {
        if (arguments.Length == 2
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg0)
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2) {
        if (arguments.Length == 3
            && TryResolveImmediateOrVariable(arguments[0], scope, out arg0)
            && TryResolveImmediateOrVariable(arguments[1], scope, out arg1)
            && TryResolveImmediateOrVariable(arguments[2], scope, out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private static bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, Scope scope, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3) {
        if (arguments.Length == 4
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
}
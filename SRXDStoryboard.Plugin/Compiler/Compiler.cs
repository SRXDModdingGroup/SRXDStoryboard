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
        
        var procedures = new Dictionary<string, int>();

        for (int i = 0; i < instructions.Count; i++) {
            var instruction = instructions[i];

            if (instruction.Opcode != Opcode.Proc)
                continue;
            
            if (!TryGetArguments(instruction.Arguments, null, out string[] str) || str.Length != 1) {
                ThrowCompileError(instruction.LineIndex, "Invalid arguments for instruction Proc");
                
                return false;
            }

            string name = str[0];

            if (procedures.ContainsKey(name)) {
                ThrowCompileError(instruction.LineIndex, $"Procedure {name} already exists");
                
                return false;
            }

            procedures.Add(name, i);
        }

        if (!procedures.TryGetValue("Main", out int mainIndex)) {
            ThrowCompileError(0, "Procedure Main could not be found");

            return false;
        }

        var globals = new Dictionary<string, object>();
        var currentScope = new Scope(null, Timestamp.Zero, 0, globals, new Dictionary<string, object>());
        int index = mainIndex + 1;

        while (currentScope != null) {
            if (index >= instructions.Count || instructions[index].Opcode == Opcode.Proc) {
                index = currentScope.ReturnIndex;
                currentScope = currentScope.Parent;
                
                continue;
            }
            
            var instruction = instructions[index];
            var opcode = instruction.Opcode;
            object[] arguments = instruction.Arguments;
            
            switch (opcode) {
                case Opcode.Call when TryGetArguments(arguments, currentScope, out string[] str) && str.Length == 1:
                    string name = str[0];

                    if (!procedures.TryGetValue(name, out int newIndex)) {
                        ThrowCompileError(instruction.LineIndex, $"Procedure {name} could not be found");

                        return false;
                    }

                    currentScope = new Scope(currentScope, currentScope.StartTime + instruction.Timestamp, index, globals, new Dictionary<string, object>());
                    index = newIndex;

                    break;
                case Opcode.Event:
                    break;
                case Opcode.Key:
                    break;
                case Opcode.Set when TryGetArguments(arguments, currentScope, out string[] str, out object value) && str.Length == 1:
                    currentScope.SetValue(str[0], value);

                    break;
                case Opcode.SetG when TryGetArguments(arguments, currentScope, out string[] str, out object value) && str.Length == 1:
                    globals[str[0]] = value;

                    break;
                case Opcode.Bundle:
                case Opcode.Inst:
                case Opcode.Load:
                    ThrowCompileError(instruction.LineIndex, $"Instruction {opcode} can not be used within a procedure");

                    return false;
                default:
                    ThrowCompileError(instruction.LineIndex, $"Invalid arguments for instruction {opcode}");

                    return false;
            }

            index++;
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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDStoryboard.Plugin;

public class Compiler {
    private Dictionary<string, Variable> variables;

    private Compiler() { }

    private bool TryCompile(List<Instruction> instructions, out Storyboard storyboard) {
        variables = new Dictionary<string, Variable>();
        
        foreach (var instruction in instructions) {
            object[] arguments = instruction.Arguments;
            
            switch (instruction.Opcode) {
                case Opcode.Def when TryGetArguments(arguments, out string[] str, out Variable variable):
                    if (str.Length != 1)
                        break;

                    string name = str[0];

                    if (variables.ContainsKey(name))
                        break;
                    
                    variables.Add(name, variable);
                    
                    break;
                case Opcode.Event:
                    break;
                case Opcode.Inst:
                    break;
                case Opcode.Key:
                    break;
                case Opcode.Load:
                    break;
                case Opcode.Post:
                    break;
                case Opcode.Proc:
                    break;
            }
        }
    }

    private bool TryGetArguments(object[] arguments) => arguments.Length == 0;
    private bool TryGetArguments<T>(object[] arguments, out T arg) {
        if (arguments.Length == 1 && TryResolveImmediateOrVariable(arguments[0], out arg))
            return true;

        arg = default;

        return false;
    }
    private bool TryGetArguments<T0, T1>(object[] arguments, out T0 arg0, out T1 arg1) {
        if (arguments.Length == 2
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[0], out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private bool TryGetArguments<T0, T1, T2>(object[] arguments, out T0 arg0, out T1 arg1, out T2 arg2) {
        if (arguments.Length == 3
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[1], out arg1)
            && TryResolveImmediateOrVariable(arguments[2], out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3) {
        if (arguments.Length == 4
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[1], out arg1)
            && TryResolveImmediateOrVariable(arguments[2], out arg2)
            && TryResolveImmediateOrVariable(arguments[3], out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }

    private bool TryResolveImmediateOrVariable<T>(object argument, out T value) {
        if (Conversion.TryConvert(argument, out value))
            return true;

        if (argument is not string[] hierarchy || !variables.TryGetValue(hierarchy[0], out var variable)) {
            value = default;

            return false;
        }

        for (int i = 1; i < hierarchy.Length; i++) {
            if (!variable.TryGetSubVariable(hierarchy[i], out var temp)) {
                value = default;

                return false;
            }

            variable = temp;
        }

        if (variable is T cast) {
            value = cast;

            return true;
        }

        if (Conversion.TryConvert(variable.Value, out value))
            return true;

        value = default;

        return false;
    }

    public static bool TryCompileFile(string path, out Storyboard storyboard) {
        if (Parser.TryParseFile(path, out var instructions))
            return new Compiler().TryCompile(instructions, out storyboard);
        
        storyboard = null;
            
        return false;
    }
}
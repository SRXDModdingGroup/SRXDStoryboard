using System.Collections.Generic;
using UnityEngine;

namespace SRXDStoryboard.Plugin;

public class Compiler {
    private Dictionary<string, Variable> variables;

    private Compiler() { }

    private bool TryCompileInstructions(List<Instruction> instructions, out Storyboard storyboard) {
        variables = new Dictionary<string, Variable>();
        
        foreach (var instruction in instructions) {
            switch (instruction.Opcode) {
                
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
        if (TryGetConversion(argument, out value))
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

        if (TryGetConversion(variable.Value, out value))
            return true;

        value = default;

        return false;
    }

    public static bool TryCompileFile(string path, out Storyboard storyboard) {
        if (Parser.TryParseFile(path, out var instructions))
            return new Compiler().TryCompileInstructions(instructions, out storyboard);
        
        storyboard = null;
            
        return false;
    }

    private static bool TryGetConversion<T>(object value, out T conversion) {
        if (value is T cast) {
            conversion = cast;

            return true;
        }

        conversion = (T) (object) (typeof(T) switch {
            var type when type == typeof(Quaternion) => value switch {
                float f => Quaternion.Euler(0f, 0f, f),
                Vector3 v => Quaternion.Euler(v.x, v.y, v.z),
                Vector4 v => new Quaternion(v.x, v.y, v.z, v.w),
                _ => null
            },
            var type when type == typeof(Color) => value switch {
                float f => new Color(f, f, f),
                Vector3 v => new Color(v.x, v.y, v.z),
                Vector4 v => new Color(v.x, v.y, v.z, v.w),
                _ => null
            },
            _ => null
        });
        
        return conversion == null;
    }
}
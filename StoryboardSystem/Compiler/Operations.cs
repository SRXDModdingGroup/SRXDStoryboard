using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

internal static class Operations {
    private static Dictionary<string, Func<object, object, object>> BINARY_OPERATIONS = new() {
        {"Add", Add},
        {"Sub", Sub},
        {"Mult", Mult},
        {"Div", Div},
        {"Mod", Mod}
    };

    public static bool TryDoOperation(string name, object[] args, out object result) {
        if (BINARY_OPERATIONS.TryGetValue(name, out var op))
            return TryDoChainedOp(args, op, out result);

        result = null;

        return false;
    }

    private static bool TryDoBinaryOp(object a, object b, Func<object, object, object> op, out object result) {
        if (a is not object[] && b is not object[])
            result = op(a, b);
        else if (a is not object[] && b is object[] arr0) {
            object[] newArr = new object[arr0.Length];

            for (int i = 0; i < arr0.Length; i++) {
                object newVal = op(a, arr0[i]);

                if (newVal == null) {
                    result = null;
                    
                    return false;
                }

                newArr[i] = newVal;
            }

            result = newArr;
        }
        else if (a is object[] arr1 && b is not object[]) {
            object[] newArr = new object[arr1.Length];

            for (int i = 0; i < arr1.Length; i++) {
                object newVal = op(arr1[i], b);

                if (newVal == null) {
                    result = null;
                    
                    return false;
                }

                newArr[i] = newVal;
            }

            result = newArr;
        }
        else if (a is object[] arr2 && b is object[] arr3) {
            if (arr2.Length != arr3.Length) {
                result = null;

                return false;
            }

            object[] newArr = new object[arr2.Length];

            for (int i = 0; i < arr2.Length; i++) {
                object newVal = op(arr2[i], arr3[i]);

                if (newVal == null) {
                    result = null;

                    return false;
                }

                newArr[i] = newVal;
            }

            result = newArr;
        }
        else
            result = null;

        return result != null;
    }

    private static bool TryDoChainedOp(object[] args, Func<object, object, object> op, out object result) {
        if (args.Length < 2) {
            result = null;
            
            return false;
        }

        result = args[0];

        for (int i = 1; i < args.Length; i++) {
            if (!TryDoBinaryOp(result, args[i], op, out object temp)) {
                result = null;

                return false;
            }

            result = temp;
        }

        return true;
    }
    
    private static object Add(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 + int1,
        (int int0, float float1) => int0 + float1,
        (float float0, int int1) => float0 + int1,
        (float float0, float float1) => float0 + float1,
        _ => null
    };
    
    private static object Sub(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 - int1,
        (int int0, float float1) => int0 - float1,
        (float float0, int int1) => float0 - int1,
        (float float0, float float1) => float0 - float1,
        _ => null
    };
    
    private static object Mult(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 * int1,
        (int int0, float float1) => int0 * float1,
        (float float0, int int1) => float0 * int1,
        (float float0, float float1) => float0 * float1,
        _ => null
    };
    
    private static object Div(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 / int1,
        (int int0, float float1) => int0 / float1,
        (float float0, int int1) => float0 / int1,
        (float float0, float float1) => float0 / float1,
        _ => null
    };
    
    private static object Mod(object a, object b) => (a, b) switch {
        (int int0, int int1) => MathUtility.Mod(int0, int1),
        _ => null
    };
}
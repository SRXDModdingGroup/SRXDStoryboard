using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

internal static class Functions {
    private static readonly float TWO_PI = 2f * Mathf.PI;
    
    private static Dictionary<FuncName, Func<object, object, object>> BINARY_FUNCTIONS = new() {
        { FuncName.Arr, Arr }
    };
    
    private static Dictionary<FuncName, Func<object, object, object, object>> TERNARY_FUNCTIONS = new() {
        { FuncName.Lerp, Lerp },
        { FuncName.ILerp, ILerp }
    };
    
    private static Dictionary<FuncName, Func<object, object>> UNARY_MATH_FUNCTIONS = new() {
        { FuncName.Abs, Abs },
        { FuncName.Sign, Sign },
        { FuncName.Floor, Floor },
        { FuncName.Ceil, Ceil },
        { FuncName.Round, Round },
        { FuncName.Sqrt, Sqrt },
        { FuncName.Sin, Sin },
        { FuncName.Cos, Cos }
    };
    
    private static Dictionary<FuncName, Func<object, object, object>> BINARY_MATH_FUNCTIONS = new() {
        { FuncName.Mod, Mod }
    };
    
    private static Dictionary<FuncName, Func<object, object, object>> CHAINABLE_MATH_FUNCTIONS = new() {
        { FuncName.Add, Add },
        { FuncName.Sub, Sub },
        { FuncName.Mult, Mult },
        { FuncName.Div, Div },
        { FuncName.DivF, DivF },
        { FuncName.Min, Min },
        { FuncName.Max, Max }
    };

    public static bool TryDoFunction(FuncName name, object[] args, ILogger logger, out object result) {
        if (BINARY_FUNCTIONS.TryGetValue(name, out var binOp)) {
            if (args.Length == 2)
                return TryDoBinaryOp(args[0], args[1], binOp, out result);
        }
        else if (TERNARY_FUNCTIONS.TryGetValue(name, out var ternOp)) {
            if (args.Length == 3)
                return TryDoTernaryOp(args[0], args[1], args[2], ternOp, out result);
        }
        else if (UNARY_MATH_FUNCTIONS.TryGetValue(name, out var unMOp)) {
            if (args.Length == 1)
                return TryDoUnaryMathOp(args[0], unMOp, out result);
        }
        else if (BINARY_MATH_FUNCTIONS.TryGetValue(name, out var binMOp)) {
            if (args.Length == 2)
                return TryDoBinaryMathOp(args[0], args[1], binMOp, out result);
        }
        else if (CHAINABLE_MATH_FUNCTIONS.TryGetValue(name, out var chMOp)) {
            if (args.Length == 2)
                return TryDoBinaryMathOp(args[0], args[1], chMOp, out result);
            
            if (args.Length > 2)
                return TryDoChainedMathOp(args, chMOp, out result);
        }

        result = null;
        logger.LogWarning($"Function {name} not found");

        return false;
    }

    private static bool TryDoBinaryOp(object a, object b, Func<object, object, object> op, out object result) {
        result = op(a, b);

        return result != null;
    }

    private static bool TryDoTernaryOp(object a, object b, object c, Func<object, object, object, object> op, out object result) {
        result = op(a, b, c);

        return result != null;
    }
    
    private static bool TryDoUnaryMathOp(object a, Func<object, object> op, out object result) {
        if (a is not object[] arr) {
            result = op(a);

            return result != null;
        }

        object[] newArr = new object[arr.Length];

        for (int i = 0; i < arr.Length; i++) {
            object newVal = op(arr[i]);

            if (newVal == null) {
                result = null;
                    
                return false;
            }

            newArr[i] = newVal;
        }

        result = newArr;

        return true;
    }

    private static bool TryDoBinaryMathOp(object a, object b, Func<object, object, object> op, out object result) {
        if (a is not object[] && b is not object[]) {
            result = op(a, b);

            return result != null;
        }

        if (a is not object[] && b is object[] arr0) {
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

            return true;
        }

        if (a is object[] arr1 && b is not object[]) {
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

            return true;
        }

        if (a is object[] arr2 && b is object[] arr3) {
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

            return true;
        }

        result = null;

        return false;
    }

    private static bool TryDoChainedMathOp(object[] args, Func<object, object, object> op, out object result) {
        result = args[0];

        for (int i = 1; i < args.Length; i++) {
            if (!TryDoBinaryMathOp(result, args[i], op, out object temp)) {
                result = null;

                return false;
            }

            result = temp;
        }

        return true;
    }
    
    private static object Arr(object value, object count) {
        switch (value, count) {
            case (null, int int0):
                return new object[int0];
            case (not null, int int1 and > 0):
                object[] newArr = new object[int1];

                for (int i = 0; i < int1; i++)
                    newArr[i] = value;

                return newArr;
        }

        return null;
    }

    private static object Lerp(object a, object b, object t) {
        if (!TryRaiseToFloat(t, out float floatT))
            return null;

        if (TryRaiseToFloat(a, out float floatA0) && TryRaiseToFloat(b, out float floatB0))
            return Mathf.Lerp(floatA0, floatB0, floatT);
        
        if (a is not object[] arrA || b is not object[] arrB || arrA.Length != arrB.Length)
            return null;

        object[] newArr = new object[arrA.Length];

        for (int i = 0; i < arrA.Length; i++) {
            if (!TryRaiseToFloat(arrA[i], out float floatA1) || !TryRaiseToFloat(arrB[i], out float floatB1))
                return null;

            newArr[i] = Mathf.Lerp(floatA1, floatB1, floatT);
        }

        return newArr;
    }

    private static object ILerp(object a, object b, object x) {
        if (!TryRaiseToFloat(a, out float floatA) || !TryRaiseToFloat(b, out float floatB))
            return null;

        if (TryRaiseToFloat(x, out float floatX0))
            return Mathf.InverseLerp(floatX0, floatA, floatB);
        
        if (x is not object[] arr)
            return null;

        object[] newArr = new object[arr.Length];

        for (int i = 0; i < arr.Length; i++) {
            if (!TryRaiseToFloat(arr[i], out float floatX1))
                return null;

            newArr[i] = Mathf.InverseLerp(floatA, floatB, floatX1);
        }

        return newArr;
    }

    private static object Abs(object value) => value switch {
        int intVal => Math.Abs(intVal),
        float floatVal => Mathf.Abs(floatVal),
        _ => null
    };

    private static object Sign(object value) => value switch {
        int intVal => Math.Sign(intVal),
        float floatVal => Mathf.Sign(floatVal),
        _ => null
    };
    
    private static object Floor(object value) => value switch {
        int intVal => intVal,
        float floatVal => Mathf.FloorToInt(floatVal),
        _ => null
    };
    
    private static object Ceil(object value) => value switch {
        int intVal => intVal,
        float floatVal => Mathf.CeilToInt(floatVal),
        _ => null
    };
    
    private static object Round(object value) => value switch {
        int intVal => intVal,
        float floatVal => Mathf.RoundToInt(floatVal),
        _ => null
    };

    private static object Sqrt(object value) => value switch {
        int intVal and >= 0 => Mathf.Sqrt(intVal),
        float floatVal and >= 0f => Mathf.Sqrt(floatVal),
        _ => null
    };

    private static object Sin(object value) => value switch {
        int intVal => Mathf.Sin(TWO_PI * intVal),
        float floatVal => Mathf.Sin(TWO_PI * floatVal),
        _ => null
    };
    
    private static object Cos(object value) => value switch {
        int intVal => Mathf.Cos(TWO_PI * intVal),
        float floatVal => Mathf.Cos(TWO_PI * floatVal),
        _ => null
    };
    
    private static object Mod(object a, object b) => (a, b) switch {
        (int int0, int int1) => MathUtility.Mod(int0, int1),
        _ => null
    };
    
    private static object Add(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 + int1,
        (int int0, float float1) => int0 + float1,
        (float float0, int int1) => float0 + int1,
        (float float0, float float1) => float0 + float1,
        (Timestamp time0, Timestamp time1) => time0 + time1,
        _ => null
    };
    
    private static object Sub(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 - int1,
        (int int0, float float1) => int0 - float1,
        (float float0, int int1) => float0 - int1,
        (float float0, float float1) => float0 - float1,
        (Timestamp time0, Timestamp time1) => time0 - time1,
        _ => null
    };
    
    private static object Mult(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 * int1,
        (int int0, float float1) => int0 * float1,
        (int int0, Timestamp time1) => int0 * time1,
        (float float0, int int1) => float0 * int1,
        (float float0, float float1) => float0 * float1,
        (float float0, Timestamp time1) => float0 * time1,
        (Timestamp time0, int int1) => time0 * int1,
        (Timestamp time0, float float1) => time0 * float1, 
        _ => null
    };
    
    private static object Div(object a, object b) => (a, b) switch {
        (int int0, int int1) => int0 / int1,
        (int int0, float float1) => int0 / float1,
        (float float0, int int1) => float0 / int1,
        (float float0, float float1) => float0 / float1,
        (Timestamp time0, int int1) => time0 / int1,
        (Timestamp time0, float float1) => time0 / float1,
        _ => null
    };
    
    private static object DivF(object a, object b) => (a, b) switch {
        (int int0, int int1) => (float) int0 / int1,
        (int int0, float float1) => int0 / float1,
        (float float0, int int1) => float0 / int1,
        (float float0, float float1) => float0 / float1,
        _ => null
    };
    
    private static object Min(object a, object b) => (a, b) switch {
        (int int0, int int1) => Math.Min(int0, int1),
        (int int0, float float1) => Mathf.Min(int0, float1),
        (float float0, int int1) => Mathf.Min(float0, int1),
        (float float0, float float1) => Mathf.Min(float0, float1),
        _ => null
    };
    
    private static object Max(object a, object b) => (a, b) switch {
        (int int0, int int1) => Math.Max(int0, int1),
        (int int0, float float1) => Mathf.Max(int0, float1),
        (float float0, int int1) => Mathf.Max(float0, int1),
        (float float0, float float1) => Mathf.Max(float0, float1),
        _ => null
    };

    private static bool TryRaiseToFloat(object obj, out float value) {
        switch (obj) {
            case int intVal:
                value = intVal;
                return true;
            case float floatVal:
                value = floatVal;
                return true;
        }

        value = 0f;

        return false;
    }
}
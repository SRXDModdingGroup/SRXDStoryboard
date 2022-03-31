using System;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class Conversion {
    private static readonly Conversion[] CONVERSIONS = {
        new Conversion<Quaternion>(val => val switch {
            float f => (Quaternion.Euler(0f, 0f, f), true),
            Vector3 v => (Quaternion.Euler(v.x, v.y, v.z), true),
            Vector4 v => (new Quaternion(v.x, v.y, v.z, v.w), true),
            _ => (default, false)
        }),
        new Conversion<Color>(val => val switch {
            float f => (new Color(f, f, f), true),
            Vector3 v => (new Color(v.x, v.y, v.z), true),
            Vector4 v => (new Color(v.x, v.y, v.z, v.w), true),
            _ => (default, false)
        })
    };
    
    protected Conversion() { }

    public static bool TryConvert<T>(object value, out T result) {
        if (value is T cast) {
            result = cast;

            return true;
        }

        foreach (var conversion in CONVERSIONS) {
            if (conversion is not Conversion<T> typed)
                continue;

            (result, bool success) = typed.Convert(value);

            return success;
        }

        result = default;

        return false;
    }
}

public class Conversion<T> : Conversion {
    public Func<object, (T result, bool success)> Convert { get; }

    public Conversion(Func<object, (T result, bool success)> convert) => Convert = convert;
}
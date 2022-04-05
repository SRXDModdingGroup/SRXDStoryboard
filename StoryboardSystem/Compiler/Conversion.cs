using UnityEngine;

namespace StoryboardSystem; 

public static class Conversion {
    public static bool TryConvertToFloat(object value, out float result) {
        switch (value) {
            case float floatVal:
                result = floatVal;

                return true;
            case int intVal:
                result = intVal;

                return true;
            case bool boolVal:
                result = boolVal ? 1f : 0f;

                return true;
            default:
                result = 0f;

                return false;
        }
    }

    public static bool TryConvertToVector(object value, out VectorN result) {
        if (value is object[] arr) {
            if (arr.Length is 0 or > 4) {
                result = default;
                
                return false;
            }
                
            float x = 0f;
            float y = 0f;
            float z = 0f;
            float w = 0f;
            int dimensions = arr.Length;

            if (dimensions >= 1 && !TryConvertToFloat(arr[0], out x)
                || dimensions >= 2 && !TryConvertToFloat(arr[1], out y)
                || dimensions >= 3 && !TryConvertToFloat(arr[2], out z)
                || dimensions >= 4 && !TryConvertToFloat(arr[3], out w)) {
                result = default;
                
                return false;
            }

            result = new VectorN(new Vector4(x, y, z, w), dimensions);
        }
        else if (TryConvertToFloat(value, out float x))
            result = new VectorN(new Vector4(x, 0f, 0f, 0f), 1);
        else {
            result = default;
                
            return false;
        }

        return true;
    }
}
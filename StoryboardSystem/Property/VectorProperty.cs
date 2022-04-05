using UnityEngine;

namespace StoryboardSystem; 

internal abstract class VectorProperty<T> : ValueProperty<T> {
    public override bool TryConvert(object value, out T result) {
        if (Conversion.TryConvertToVector(value, out var vector))
            return TryConvert(vector.Value, vector.Dimensions, out result);
        
        result = default;

        return false;

    }

    protected abstract bool TryConvert(Vector4 value, int dimensions, out T result);
}
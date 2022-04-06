using UnityEngine;

namespace StoryboardSystem; 

internal readonly struct VectorN {
    public Vector4 Value { get; }
    
    public int Dimensions { get; }

    public VectorN(Vector4 value, int dimensions) {
        Value = value;
        Dimensions = dimensions;
    }
}
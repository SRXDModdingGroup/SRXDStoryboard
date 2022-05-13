using UnityEngine;

namespace StoryboardSystem.Core; 

public class Keyframe {
    public double Time { get; }
    
    public Vector4 Value { get; }
    
    public InterpType InterpType { get; }

    public Keyframe(double time, Vector4 value, InterpType interpType) {
        Time = time;
        Value = value;
        InterpType = interpType;
    }
}
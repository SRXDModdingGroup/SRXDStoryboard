using UnityEngine;

namespace StoryboardSystem.Core; 

public class Keyframe {
    public double Time { get; }
    
    public Vector3 Value { get; }
    
    public InterpType InterpType { get; }

    public Keyframe(double time, Vector3 value, InterpType interpType) {
        Time = time;
        Value = value;
        InterpType = interpType;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class Keyframe {
    public double Time { get; }
    
    public List<Vector3> Parameters { get; }
    
    public InterpType InterpType { get; }

    public Keyframe(double time, List<Vector3> parameters, InterpType interpType) {
        Time = time;
        Parameters = parameters;
        InterpType = interpType;
    }
}
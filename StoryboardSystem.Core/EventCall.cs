using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class EventCall {
    public double Time { get; }
    
    public List<Vector4> Parameters { get; }

    public EventCall(double time, List<Vector4> parameters) {
        Time = time;
        Parameters = parameters;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class EventCall {
    public double Time { get; }
    
    public List<Vector3> Parameters { get; }

    public EventCall(double time, List<Vector3> parameters) {
        Time = time;
        Parameters = parameters;
    }
}
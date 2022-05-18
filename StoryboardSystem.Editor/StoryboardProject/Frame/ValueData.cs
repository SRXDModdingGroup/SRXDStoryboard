using UnityEngine;

namespace StoryboardSystem.Editor; 

public struct ValueData {
    public Vector4 Start { get; set; }
    
    public Vector4 EndOrStep { get; set; }
    
    public ValueDataType Type { get; set; }

    public ValueData() {
        Start = Vector4.zero;
        EndOrStep = Vector4.zero;
        Type = ValueDataType.Fixed;
    }

    public ValueData(Vector4 start, Vector4 endOrStep, ValueDataType type) {
        Start = start;
        EndOrStep = endOrStep;
        Type = type;
    }
}
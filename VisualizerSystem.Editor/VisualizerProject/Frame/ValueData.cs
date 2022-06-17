using UnityEngine;

namespace VisualizerSystem.Editor; 

public struct ValueData {
    public Vector3 Start { get; set; }
    
    public Vector3 EndOrStep { get; set; }
    
    public ValueDataType Type { get; set; }

    public ValueData() {
        Start = Vector3.zero;
        EndOrStep = Vector3.zero;
        Type = ValueDataType.Fixed;
    }

    public ValueData(Vector3 start, Vector3 endOrStep, ValueDataType type) {
        Start = start;
        EndOrStep = endOrStep;
        Type = type;
    }
}
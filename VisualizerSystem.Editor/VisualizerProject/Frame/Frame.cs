using System.Collections.Generic;

namespace VisualizerSystem.Editor; 

public class Frame : ITimedElement {
    public double Time { get; set; }
    
    public FrameData Data { get; set; }
    
    public List<ValueData> Values { get; }

    public Frame(double time, FrameData data, List<ValueData> values) {
        Time = time;
        Data = data;
        Values = values;
    }
}
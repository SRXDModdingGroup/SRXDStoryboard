using System;
using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class Frame : IComparable<Frame> {
    public double Time { get; set; }
    
    public FrameData Data { get; set; }
    
    public InterpType InterpType { get; set; }
    
    public List<ValueData> Values { get; }

    public Frame(double time, FrameData data, InterpType interpType, List<ValueData> values) {
        Time = time;
        Data = data;
        InterpType = interpType;
        Values = values;
    }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);
}
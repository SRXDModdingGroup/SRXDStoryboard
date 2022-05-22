using System;
using System.Collections.Generic;
using StoryboardSystem.Core;

namespace StoryboardSystem.Editor; 

public class Frame : IComparable<Frame> {
    public double Time { get; set; }
    
    public int EventIndex { get; }
    
    public FrameData Data { get; set; }
    
    public InterpType InterpType { get; set; }
    
    public List<ValueData> Values { get; }

    public Frame(double time, int eventIndex, FrameData data, InterpType interpType, List<ValueData> values) {
        Time = time;
        EventIndex = eventIndex;
        Data = data;
        InterpType = interpType;
        Values = values;
    }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);
}
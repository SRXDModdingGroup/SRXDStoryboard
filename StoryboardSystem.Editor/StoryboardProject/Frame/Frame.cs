using System;
using System.Collections.Generic;
using StoryboardSystem.Core;

namespace StoryboardSystem.Editor; 

public class Frame : IComparable<Frame> {
    public double Time { get; set; }
    
    public FrameData Data { get; set; }
    
    public List<ValueData> Values { get; }

    public Frame(double time, FrameData data, List<ValueData> values) {
        Time = time;
        Data = data;
        Values = values;
    }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);
}
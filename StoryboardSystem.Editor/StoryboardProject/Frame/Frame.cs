using System;

namespace StoryboardSystem.Editor; 

public abstract class Frame : IComparable<Frame> {
    public double Time { get; set; }
    
    public FrameData Data { get; }

    protected Frame(double time, FrameData data) {
        Time = time;
        Data = data;
    }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);
}
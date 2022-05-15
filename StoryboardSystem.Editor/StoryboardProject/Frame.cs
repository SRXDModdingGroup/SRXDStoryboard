using System;

namespace StoryboardSystem.Editor; 

public abstract class Frame : IComparable<Frame> {
    public double Time { get; set; }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);
}
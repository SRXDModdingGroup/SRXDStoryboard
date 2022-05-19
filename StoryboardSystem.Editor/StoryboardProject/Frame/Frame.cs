using System;
using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public abstract class Frame : IComparable<Frame> {
    public double Time { get; set; }
    
    public FrameData Data { get; }

    protected Frame(double time, FrameData data) {
        Time = time;
        Data = data;
    }

    public int CompareTo(Frame other) => Time.CompareTo(other.Time);

    public static double GetMaxTime(IEnumerable<Frame> frames) {
        double maxTime = 0d;
        
        foreach (var frame in frames) {
            if (frame.Time > maxTime)
                maxTime = frame.Time;
        }

        return maxTime;
    }
}
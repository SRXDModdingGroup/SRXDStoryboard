using System;

namespace StoryboardSystem.Editor; 

public class PatternInstance : IComparable<PatternInstance> {
    public int PatternIndex { get; set; }
    
    public double Time { get; set; }
    
    public double CropStart { get; set; }
    
    public double CropEnd { get; set; }
    
    public int Lane { get; set; }

    public PatternInstance(int patternIndex, double time, double cropStart, double cropEnd, int lane) {
        PatternIndex = patternIndex;
        Time = time;
        CropStart = cropStart;
        CropEnd = cropEnd;
        Lane = lane;
    }

    public int CompareTo(PatternInstance other) {
        int timeComparison = Time.CompareTo(other.Time);

        if (timeComparison == 0)
            return Lane.CompareTo(other.Lane);
        
        return timeComparison;
    }
}
using System;

namespace StoryboardSystem.Editor; 

public class PatternInstance : IComparable<PatternInstance> {
    public Pattern Pattern { get; set; }
    
    public double Time { get; set; }
    
    public double CropStart { get; set; }
    
    public double CropEnd { get; set; }
    
    public int Lane { get; set; }

    public PatternInstance(Pattern pattern, double time, double cropStart, double cropEnd, int lane) {
        Pattern = pattern;
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
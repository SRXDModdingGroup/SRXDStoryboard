namespace StoryboardSystem.Editor; 

public class PatternInstance {
    public double Time { get; set; }
    
    public double CropStart { get; set; }
    
    public double CropEnd { get; set; }
    
    public int Lane { get; set; }

    public PatternInstance(double time, double cropStart, double cropEnd, int lane) {
        Time = time;
        CropStart = cropStart;
        CropEnd = cropEnd;
        Lane = lane;
    }
}
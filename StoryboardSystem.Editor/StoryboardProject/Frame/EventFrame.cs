namespace StoryboardSystem.Editor; 

public class EventFrame : Frame {
    public ValueData[] Values { get; }

    public EventFrame(double time, FrameData data, int valueCount) : base(time, data) => Values = new ValueData[valueCount];
}
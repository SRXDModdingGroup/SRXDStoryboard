namespace StoryboardSystem.Editor; 

public class PropertyFrame : Frame {
    public ValueData Value { get; set; }
    
    public InterpType InterpType { get; set; }

    public PropertyFrame(double time, FrameData data, ValueData value, InterpType interpType) : base(time, data) {
        Value = value;
        InterpType = interpType;
    }
}
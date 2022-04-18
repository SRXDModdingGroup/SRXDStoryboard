namespace StoryboardSystem; 

internal readonly struct KeyframeBuilder {
    public Timestamp Time { get; }
    
    public object Value { get; }
    
    public InterpType InterpType { get; }

    public KeyframeBuilder(Timestamp time, object value, InterpType interpType) {
        Time = time;
        Value = value;
        InterpType = interpType;
    }
}
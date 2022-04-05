namespace StoryboardSystem; 

internal readonly struct EventFrameBuilder {
    private readonly Timestamp time;
    private readonly object value;

    public EventFrameBuilder(Timestamp time, object value) {
        this.time = time;
        this.value = value;
    }
    
    public bool TryCreateEventFrame<T>(EventProperty<T> property, ITimeConversion conversion, out EventFrame<T> result) {
        if (!property.TryConvert(value, out var converted)) {
            result = default;

            return false;
        }
        
        result = new EventFrame<T>(conversion.Convert(time.Beats, time.Ticks, time.Seconds), converted);

        return true;
    }
}
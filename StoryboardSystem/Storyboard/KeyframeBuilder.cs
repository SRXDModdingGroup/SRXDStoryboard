namespace StoryboardSystem; 

internal readonly struct KeyframeBuilder {
    private readonly Timestamp time;
    private readonly object value;
    private readonly InterpType interpType;
    private readonly int order;

    public KeyframeBuilder(Timestamp time, object value, InterpType interpType, int order) {
        this.time = time;
        this.value = value;
        this.interpType = interpType;
        this.order = order;
    }

    public bool TryCreateKeyframe<T>(Property<T> property, ITimeConversion conversion, out Keyframe<T> result) {
        if (!property.TryConvert(value, out var converted)) {
            result = default;

            return false;
        }
        
        result = new Keyframe<T>(conversion.Convert(time.Beats, time.Ticks, time.Seconds), converted, interpType, order);

        return true;
    }
}
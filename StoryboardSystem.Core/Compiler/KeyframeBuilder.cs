namespace StoryboardSystem.Core; 

internal readonly struct KeyframeBuilder<T> {
    private readonly T value;
    private readonly Timestamp time;
    private readonly InterpType interpType;
    private readonly int order;

    public KeyframeBuilder(T value, Timestamp time, InterpType interpType, int order) {
        this.value = value;
        this.time = time;
        this.interpType = interpType;
        this.order = order;
    }

    public Keyframe<T> CreateKeyframe(ITimeConversion conversion)
        => new(value, conversion.Convert(time.Beats, time.Ticks, time.Seconds), interpType, order);
}
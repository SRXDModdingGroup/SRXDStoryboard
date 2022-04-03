using UnityEngine;

namespace StoryboardSystem; 

internal readonly struct KeyframeBuilder {
    private readonly Timestamp time;
    private readonly VectorN value;
    private readonly InterpType interpType;
    private readonly int order;

    public KeyframeBuilder(Timestamp time, VectorN value, InterpType interpType, int order) {
        this.time = time;
        this.value = value;
        this.interpType = interpType;
        this.order = order;
    }

    public override string ToString() {
        return $"{time.Beats}b{time.Ticks:F}t{time.Seconds:F}s{time.Order}o";
    }

    public Keyframe<T> CreateKeyframe<T>(ValueProperty<T> property, ITimeConversion conversion)
        => new(conversion.Convert(time.Beats, time.Ticks, time.Seconds), property.Convert(value.Value, value.Dimensions), interpType, order);
}
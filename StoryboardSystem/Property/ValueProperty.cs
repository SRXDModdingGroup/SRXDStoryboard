namespace StoryboardSystem;

internal abstract class ValueProperty<T> : Property<T> {
    public abstract T Interp(T a, T b, float t);

    protected override Timeline CreateTimeline(Property<T>[] properties, Keyframe<T>[] keyframes) => new Curve<T>(properties, keyframes);
}
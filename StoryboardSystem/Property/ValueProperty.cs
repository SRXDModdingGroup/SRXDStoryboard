namespace StoryboardSystem;

internal abstract class ValueProperty<T> : Property<T> {
    protected abstract T Interp(T a, T b, float t);

    protected override Binding CreateBinding(Property<T>[] properties, Keyframe<T>[] keyframes)
        => new Binding<T>(false, properties, new CurveController<T>(keyframes, ((ValueProperty<T>) properties[0]).Interp));
}
namespace StoryboardSystem;

internal abstract class EventProperty : Property<object> {
    public override void Set(object value) => Execute();

    protected override Binding CreateBinding(Property<object>[] properties, Keyframe<object>[] keyframes)
        => new Binding<object>(true, properties, new EventController<object>(keyframes));

    protected abstract void Execute();
}

internal abstract class EventProperty<T> : Property<T> {
    public override void Set(T value) => Execute(value);

    protected override Binding CreateBinding(Property<T>[] properties, Keyframe<T>[] keyframes)
        => new Binding<T>(true, properties, new EventController<T>(keyframes));

    protected abstract void Execute(T value);
}
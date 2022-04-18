namespace StoryboardSystem;

internal abstract class EventProperty : Property<object> {
    public override bool IsEvent => true;

    public override void Set(object value) => Execute();

    public override object Interpolate(object a, object b, float t) => a;

    protected abstract void Execute();
}

internal abstract class EventProperty<T> : Property<T> {
    public override bool IsEvent => true;
    
    public override void Set(T value) => Execute(value);

    public override T Interpolate(T a, T b, float t) => a;

    protected abstract void Execute(T value);
}
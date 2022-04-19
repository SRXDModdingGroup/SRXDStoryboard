namespace StoryboardSystem;

internal abstract class EventProperty : Property<object> {
    protected internal override bool IsEvent => true;

    protected internal override void Set(object value) => Execute();

    protected internal override object Interpolate(object a, object b, float t) => a;

    protected abstract void Execute();
}

internal abstract class EventProperty<T> : Property<T> {
    protected internal override bool IsEvent => true;
    
    protected internal override void Set(T value) => Execute(value);

    protected internal override T Interpolate(T a, T b, float t) => a;

    protected abstract void Execute(T value);
}
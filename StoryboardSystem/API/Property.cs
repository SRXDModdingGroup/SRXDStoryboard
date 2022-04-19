namespace StoryboardSystem;

public abstract class Property {
    protected internal abstract bool IsEvent { get; }
    
    internal Property() { }

    protected internal abstract void Reset();

    internal abstract bool TryCreateTimeline(TimelineBuilder builder, IStoryboardParams sParams, out Controller controller);
}

public abstract class Property<T> : Property {
    protected internal abstract void Set(T value);

    protected internal abstract bool TryConvert(object value, out T result);

    protected internal abstract T Interpolate(T a, T b, float t);

    internal override bool TryCreateTimeline(TimelineBuilder builder, IStoryboardParams sParams, out Controller controller)
        => builder.TryCreateController(this, sParams, out controller);
}
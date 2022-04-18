namespace StoryboardSystem;

public abstract class Property {
    internal Property() { }

    internal abstract bool TryCreateTimeline(TimelineBuilder builder, IStoryboardParams sParams, out Controller controller);
}

public abstract class Property<T> : Property {
    public abstract bool IsEvent { get; }
    
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);

    public abstract T Interpolate(T a, T b, float t);

    internal override bool TryCreateTimeline(TimelineBuilder builder, IStoryboardParams sParams, out Controller controller)
        => builder.TryCreateController(this, sParams, out controller);
}
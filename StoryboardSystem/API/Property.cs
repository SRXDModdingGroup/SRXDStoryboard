namespace StoryboardSystem; 

public abstract class Property { }

public abstract class Property<T> : Property {
    public abstract bool IsEvent { get; }
    
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);

    public abstract T Interpolate(T a, T b, float t);
}
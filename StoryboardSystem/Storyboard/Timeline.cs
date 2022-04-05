namespace StoryboardSystem; 

internal abstract class Timeline {
    public abstract void Evaluate(float time);
}

internal abstract class Timeline<T> : Timeline {
    protected Keyframe<T>[] Keyframes { get; }
    
    private Property<T>[] properties;

    protected Timeline(Property<T>[] properties, Keyframe<T>[] keyframes) {
        this.properties = properties;
        Keyframes = keyframes;
    }

    protected void Set(T value) {
        foreach (var property in properties)
            property.Set(value);
    }
}
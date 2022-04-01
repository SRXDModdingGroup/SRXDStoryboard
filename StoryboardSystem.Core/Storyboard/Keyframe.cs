using System;

namespace StoryboardSystem.Core; 

internal readonly struct Keyframe<T> : IComparable<Keyframe<T>> {
    public T Value { get; }
    
    public float Time { get; }
    
    public InterpType InterpType { get; }

    private readonly int order;

    public Keyframe(T value, float time, InterpType interpType, int order) {
        Value = value;
        Time = time;
        InterpType = interpType;
        this.order = order;
    }

    public int CompareTo(Keyframe<T> other) {
        int timeComparison = Time.CompareTo(other.Time);
        
        if (timeComparison != 0)
            return timeComparison;
        
        return order.CompareTo(other.order);
    }
}
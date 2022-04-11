using System;

namespace StoryboardSystem; 

internal readonly struct Keyframe<T> : IComparable<Keyframe<T>> {
    public float Time { get; }
    
    public T Value { get; }
    
    public InterpType InterpType { get; }

    public int Order { get; }

    public Keyframe(float time, T value, InterpType interpType, int order) {
        Time = time;
        Value = value;
        InterpType = interpType;
        this.Order = order;
    }

    public int CompareTo(Keyframe<T> other) {
        int timeComparison = Time.CompareTo(other.Time);
        
        if (timeComparison != 0)
            return timeComparison;
        
        return Order.CompareTo(other.Order);
    }
}
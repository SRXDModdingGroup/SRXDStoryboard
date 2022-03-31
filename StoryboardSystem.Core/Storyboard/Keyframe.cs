using System;

namespace StoryboardSystem.Core; 

internal readonly struct Keyframe<T> : IComparable<Keyframe<T>> {
    public T Value { get; }
    
    public float Time { get; }
    
    public int Order { get; }
    
    public InterpType InterpType { get; }

    public Keyframe(T value, float time, int order, InterpType interpType) {
        Value = value;
        Time = time;
        Order = order;
        InterpType = interpType;
    }

    public int CompareTo(Keyframe<T> other) {
        int timeComparison = Time.CompareTo(other.Time);
        
        if (timeComparison != 0)
            return timeComparison;
        
        return Order.CompareTo(other.Order);
    }
}
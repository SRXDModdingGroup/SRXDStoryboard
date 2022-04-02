using System;

namespace StoryboardSystem.Core; 

internal readonly struct Keyframe<T> : IComparable<Keyframe<T>> {
    public float Time { get; }
    
    public T Value { get; }
    
    public InterpType InterpType { get; }

    private readonly int order;

    public Keyframe(float time, T value, InterpType interpType, int order) {
        Time = time;
        Value = value;
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
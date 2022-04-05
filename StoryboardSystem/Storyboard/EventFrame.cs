using System;

namespace StoryboardSystem; 

internal readonly struct EventFrame<T> : IComparable<EventFrame<T>> {
    public float Time { get; }
    
    public T Value { get; }

    public EventFrame(float time, T value) {
        Time = time;
        Value = value;
    }

    public int CompareTo(EventFrame<T> other) => Time.CompareTo(other.Time);
}
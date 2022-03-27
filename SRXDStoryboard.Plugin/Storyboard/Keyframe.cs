using System;

namespace SRXDStoryboard.Plugin; 

public readonly struct Keyframe<T> : IComparable<Keyframe<T>> {
    public T Value { get; }
    
    public float Time { get; }
    
    public InterpType InterpType { get; }

    public Keyframe(T value, float time, InterpType interpType) {
        Value = value;
        Time = time;
        InterpType = interpType;
    }

    public int CompareTo(Keyframe<T> other) => Time.CompareTo(other.Time);
}
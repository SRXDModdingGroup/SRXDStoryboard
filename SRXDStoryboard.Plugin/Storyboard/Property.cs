using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public abstract class Property<T> {
    public abstract void Set(T value);

    public abstract T Interp(T a, T b, float t);
}
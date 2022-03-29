using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public abstract class UnityObjectReference<T> where T : Object {
    public T Value { get; protected set; }

    public abstract void Load();

    public abstract void Unload();
}
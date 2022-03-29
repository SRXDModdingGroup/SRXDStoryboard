using UnityEngine;

namespace SRXDStoryboard.Plugin;

public abstract class ObjectReference {
    public abstract void Load();

    public abstract void Unload();
}

public abstract class ObjectReference<T> : ObjectReference where T : Object {
    public T Value { get; protected set; }
}
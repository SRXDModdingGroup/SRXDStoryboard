using UnityEngine;

namespace SRXDStoryboard.Plugin;

public abstract class LoadedObjectReference {
    public abstract void Load();

    public abstract void Unload();
}
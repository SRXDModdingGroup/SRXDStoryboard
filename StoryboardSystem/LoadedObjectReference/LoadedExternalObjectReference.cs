using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedExternalObjectReference : LoadedObjectReference {
    public override object LoadedObject => externalObject;

    private string name;
    private Object externalObject;
    
    public LoadedExternalObjectReference(string name) => this.name = name;

    public void Unload() => externalObject = null;

    public bool TryLoad(IStoryboardParams sParams, ILogger logger) {
        externalObject = sParams.GetExternalObject(name);

        if (externalObject != null)
            return true;
        
        logger.LogWarning($"Failed to get reference to external object {name}");

        return false;
    }
}
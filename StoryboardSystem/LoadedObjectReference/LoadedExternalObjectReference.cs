using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class LoadedExternalObjectReference : LoadedObjectReference {
    public override object LoadedObject => externalObject;

    private string name;
    private object externalObject;
    
    public LoadedExternalObjectReference(string name) => this.name = name;

    public override void Unload(ISceneManager sceneManager) {
        if (externalObject is IStoryboardObject customObject)
            customObject.Cleanup();
        
        externalObject = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, Dictionary<Identifier, List<Identifier>> bindings, ISceneManager sceneManager) {
        if (sceneManager.TryGetExternalObject(name, out externalObject))
            return true;

        StoryboardManager.Instance.Logger.LogWarning($"Failed to get reference to external object {name}");
        externalObject = null;

        return false;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.ExternalObject);
        writer.Write(name);

        return true;
    }

    public static LoadedExternalObjectReference Deserialize(BinaryReader reader) => new(reader.ReadString());
}
using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class LoadedExternalObjectReference : LoadedObjectReference {
    public override object LoadedObject => externalObject;

    private string key;
    private string name;
    private object externalObject;
    
    public LoadedExternalObjectReference(string name) {
        string[] split = name.Split('.');

        switch (split.Length) {
            case 1:
                key = string.Empty;
                this.name = split[0];

                return;
            case 2:
                key = split[0];
                this.name = split[1];
            
                return;
            default:
                key = string.Empty;
                this.name = name;

                return;
        }
    }

    public override void Unload(ISceneManager sceneManager) {
        if (externalObject is ICustomObject customObject)
            customObject.Cleanup();
        
        externalObject = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, Dictionary<Identifier, List<Identifier>> bindings, IStoryboardParams sParams) {
        if (StoryboardManager.Instance.Extensions.TryGetValue(key, out var extension)) {
            externalObject = extension.GetExternalObject(name);
            
            if (externalObject != null)
                return true;
        }
        
        StoryboardManager.Instance.Logger.LogWarning($"Failed to get reference to external object {name}");

        return false;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.ExternalObject);
        writer.Write(name);

        return true;
    }

    public static LoadedExternalObjectReference Deserialize(BinaryReader reader) => new(reader.ReadString());
}
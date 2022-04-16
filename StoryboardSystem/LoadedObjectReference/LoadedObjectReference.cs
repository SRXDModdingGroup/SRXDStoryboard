using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem;

internal abstract class LoadedObjectReference {
    public abstract object LoadedObject { get; }
    
    public abstract void Serialize(BinaryWriter writer);

    public abstract void Unload(ISceneManager sceneManager);

    public abstract bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger);

    public static bool TryDeserialize(BinaryReader reader, out LoadedObjectReference reference) {
        var type = (ObjectReferenceType) reader.ReadByte();
        
        switch (type) {
            case ObjectReferenceType.ExternalObject:
                reference = LoadedExternalObjectReference.Deserialize(reader);
                return true;
            case ObjectReferenceType.AssetBundle:
                reference = LoadedAssetBundleReference.Deserialize(reader);
                return true;
            case ObjectReferenceType.Asset:
                reference = LoadedAssetReference.Deserialize(reader);
                return true;
            case ObjectReferenceType.Instance:
                reference = LoadedInstanceReference.Deserialize(reader);
                return true;
            case ObjectReferenceType.PostProcessing:
                reference = LoadedPostProcessingReference.Deserialize(reader);
                return true;
            default:
                reference = null;
                return false;
        }
    }
}
using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem;

internal abstract class LoadedObjectReference {
    public abstract object LoadedObject { get; }

    public abstract void Unload(ISceneManager sceneManager);

    public abstract bool TryLoad(
        List<LoadedObjectReference> objectReferences,
        Dictionary<Identifier, List<Identifier>> bindings,
        ISceneManager sceneManager);
    
    public abstract bool TrySerialize(BinaryWriter writer);

    public static bool TryDeserialize(BinaryReader reader, out LoadedObjectReference reference) {
        byte value = reader.ReadByte();
        
        switch ((ObjectReferenceType) value) {
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
            case ObjectReferenceType.Timeline:
                return LoadedTimelineReference.TryDeserialize(reader, out reference);
            default:
                reference = null;
                StoryboardManager.Instance.Logger.LogWarning($"{value} is not a valid object reference tag");
                
                return false;
        }
    }
}
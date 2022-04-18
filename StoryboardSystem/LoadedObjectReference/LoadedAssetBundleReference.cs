using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {
    public override object LoadedObject => bundle;
    
    private string bundleName;
    private AssetBundle bundle;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Unload(ISceneManager sceneManager) {
        sceneManager.UnloadAssetBundle(bundleName);
        bundle = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, Dictionary<Identifier, List<Identifier>> bindings, ISceneManager sceneManager, IStoryboardParams sParams) {
        if (sceneManager.TryGetAssetBundle(bundleName, out bundle))
            return true;

        bundle = null;
        StoryboardManager.Instance.Logger.LogWarning($"Failed to load AssetBundle {bundleName}");

        return false;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.AssetBundle);
        writer.Write(bundleName);
        
        return true;
    }

    public static LoadedAssetBundleReference Deserialize(BinaryReader reader) => new(reader.ReadString());
}
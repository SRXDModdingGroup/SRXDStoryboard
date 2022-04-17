using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {
    public override object LoadedObject => bundle;
    
    private string bundleName;
    private AssetBundle bundle;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Serialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.AssetBundle);
        writer.Write(bundleName);
    }

    public override void Unload(ISceneManager sceneManager) {
        sceneManager.UnloadAssetBundle(bundleName);
        bundle = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams) {
        if (sceneManager.TryGetAssetBundle(bundleName, out bundle))
            return true;

        bundle = null;
        StoryboardManager.Instance.Logger.LogWarning($"Failed to load AssetBundle {bundleName}");

        return false;
    }

    public static LoadedAssetBundleReference Deserialize(BinaryReader reader) => new(reader.ReadString());
}
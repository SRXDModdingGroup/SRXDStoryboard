using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {
    public override object LoadedObject => Bundle;
    
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Serialize(BinaryWriter writer) {
        writer.Write((int) ObjectReferenceType.AssetBundle);
        writer.Write(bundleName);
    }

    public override void Unload(ISceneManager sceneManager) {
        sceneManager.UnloadAssetBundle(bundleName);
        Bundle = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (sceneManager.TryGetAssetBundle(bundleName, out var bundle)) {
            Bundle = bundle;

            return true;
        }

        Bundle = null;
        logger.LogWarning($"Failed to load AssetBundle {bundleName}");

        return false;
    }

    public static LoadedAssetBundleReference Deserialize(BinaryReader reader) => new(reader.ReadString());
}
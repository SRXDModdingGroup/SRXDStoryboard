using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal class LoadedAssetReference : LoadedObjectReference {
    public override object LoadedObject => asset;
    
    private Identifier assetBundleReference;
    private string assetName;
    private Object asset;

    public LoadedAssetReference(Identifier assetBundleReference, string assetName) {
        this.assetBundleReference = assetBundleReference;
        this.assetName = assetName;
    }

    public override void Unload(ISceneManager sceneManager) => asset = null;

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams) {
        if (!Binder.TryResolveIdentifier(assetBundleReference, objectReferences, out object obj))
            return false;

        if (obj is not AssetBundle assetBundle) {
            StoryboardManager.Instance.Logger.LogWarning($"{assetBundleReference} is not an asset bundle");

            return false;
        }
        
        asset = assetBundle.LoadAsset(assetName);

        if (asset != null)
            return true;
        
        StoryboardManager.Instance.Logger.LogWarning($"Failed to load asset {assetName}");

        return false;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.Asset);
        assetBundleReference.Serialize(writer);
        writer.Write(assetName);

        return true;
    }

    public static LoadedAssetReference Deserialize(BinaryReader reader) {
        var assetBundleReference = Identifier.Deserialize(reader);
        string assetName = reader.ReadString();

        return new LoadedAssetReference(assetBundleReference, assetName);
    }
}
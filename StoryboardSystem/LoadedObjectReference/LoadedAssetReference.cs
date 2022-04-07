using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal abstract class LoadedAssetReference : LoadedObjectReference {
    public abstract void Unload();

    public abstract bool TryLoad(ILogger logger);
    
    public abstract LoadedInstanceReference CreateInstanceReference(string name, int layer);

    public static LoadedAssetReference Create(LoadedAssetBundleReference assetBundleReference, string assetName, AssetType type) => type switch {
        AssetType.Material => new LoadedAssetReference<Material>(assetBundleReference, assetName),
        AssetType.Mesh => new LoadedAssetReference<Mesh>(assetBundleReference, assetName),
        AssetType.Prefab => new LoadedAssetReference<GameObject>(assetBundleReference, assetName),
        AssetType.Sprite => new LoadedAssetReference<Sprite>(assetBundleReference, assetName),
        AssetType.Texture => new LoadedAssetReference<Texture>(assetBundleReference, assetName),
        _ => null
    };
}

internal class LoadedAssetReference<T> : LoadedAssetReference where T : Object {
    public override Object LoadedObject => Asset;

    public T Asset { get; private set; }

    public string AssetName { get; }
    
    private LoadedAssetBundleReference assetBundleReference;

    public LoadedAssetReference(LoadedAssetBundleReference assetBundleReference, string assetName) {
        AssetName = assetName;
        this.assetBundleReference = assetBundleReference;
    }

    public override void Unload() => Asset = null;

    public override bool TryLoad(ILogger logger) {
        Asset = assetBundleReference.Bundle.LoadAsset<T>(AssetName);

        if (Asset != null)
            return true;
        
        logger.LogWarning($"Failed to load asset {AssetName}");

        return false;
    }

    public override LoadedInstanceReference CreateInstanceReference(string name, int layer) => new LoadedInstanceReference<T>(this, name, layer);
}
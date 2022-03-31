using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDStoryboard.Core;

public abstract class LoadedAssetReference : LoadedObjectReference {
    public abstract LoadedInstanceReference CreateInstanceReference();

    public static LoadedAssetReference Create(LoadedAssetBundleReference assetBundleReference, string assetName, AssetType type) => type switch {
        AssetType.Material => new LoadedAssetReference<Material>(assetBundleReference, assetName),
        AssetType.Mesh => new LoadedAssetReference<Mesh>(assetBundleReference, assetName),
        AssetType.Prefab => new LoadedAssetReference<GameObject>(assetBundleReference, assetName),
        AssetType.Sprite => new LoadedAssetReference<Sprite>(assetBundleReference, assetName),
        AssetType.Texture => new LoadedAssetReference<Texture>(assetBundleReference, assetName),
        _ => null
    };
}

public class LoadedAssetReference<T> : LoadedAssetReference where T : Object {
    public T Asset { get; private set; }
    
    private LoadedAssetBundleReference assetBundleReference;
    private string assetName;

    public LoadedAssetReference(LoadedAssetBundleReference assetBundleReference, string assetName) {
        this.assetName = assetName;
        this.assetBundleReference = assetBundleReference;
    }
    
    public override void Load() => Asset = assetBundleReference.Bundle.LoadAsset<T>(assetName);

    public override void Unload() => Asset = null;

    public override LoadedInstanceReference CreateInstanceReference() => new LoadedInstanceReference<T>(this);
}
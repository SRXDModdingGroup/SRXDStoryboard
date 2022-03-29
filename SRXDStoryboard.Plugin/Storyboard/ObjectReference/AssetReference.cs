using UnityEngine;

namespace SRXDStoryboard.Plugin;

public class AssetReference<T> : ObjectReference<T> where T : Object {
    private AssetBundleReference assetBundleReference;
    private string assetName;

    public AssetReference(AssetBundleReference assetBundleReference, string assetName) {
        this.assetName = assetName;
        this.assetBundleReference = assetBundleReference;
    }

    public override void Load() => Value = assetBundleReference.Value.LoadAsset<T>(assetName);

    public override void Unload() => Value = null;
}
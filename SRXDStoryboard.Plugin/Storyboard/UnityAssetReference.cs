using SMU;
using SpinCore.Utility;
using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class UnityAssetReference<T> : UnityObjectReference<T> where T : Object {
    private AssetBundle bundle;
    private string assetName;

    public UnityAssetReference(AssetBundle bundle, string assetName) {
        this.bundle = bundle;
        this.assetName = assetName;
    }

    public override void Load() => Value = bundle.LoadAsset<T>(assetName);

    public override void Unload() => Value = null;
}
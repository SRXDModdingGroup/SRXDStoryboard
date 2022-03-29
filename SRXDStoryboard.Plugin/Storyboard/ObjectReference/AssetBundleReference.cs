using SMU;
using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class AssetBundleReference : ObjectReference<AssetBundle> {
    private string bundleName;

    public AssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Load() {
        if (AssetBundleUtility.TryGetAssetBundle(Plugin.CustomAssetBundlePath, bundleName, out var bundle))
            Value = bundle;
        else
            Value = null;
    }

    public override void Unload() {
        Value.Unload(false);
        Value = null;
    }
}
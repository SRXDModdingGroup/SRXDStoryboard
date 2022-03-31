using SMU;
using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class LoadedAssetBundleReference : LoadedObjectReference {
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Load() {
        if (AssetBundleUtility.TryGetAssetBundle(Plugin.CustomAssetBundlePath, bundleName, out var bundle))
            Bundle = bundle;
        else
            Bundle = null;
    }

    public override void Unload() {
        Bundle.Unload(false);
        Bundle = null;
    }
}
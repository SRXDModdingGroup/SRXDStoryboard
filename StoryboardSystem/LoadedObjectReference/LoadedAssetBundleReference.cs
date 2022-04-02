using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {

    public override object LoadedObject => Bundle;
    
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Load() {
        if (StoryboardManager.Instance.AssetBundleManager.TryGetAssetBundle(bundleName, out var bundle))
            Bundle = bundle;
        else
            Bundle = null;
    }

    public override void Unload() {
        Bundle.Unload(false);
        Bundle = null;
    }
}
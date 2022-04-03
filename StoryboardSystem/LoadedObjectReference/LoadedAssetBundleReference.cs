using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {

    public override object LoadedObject => Bundle;
    
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override bool TryLoad() {
        if (StoryboardManager.Instance.AssetBundleManager.TryGetAssetBundle(bundleName, out var bundle)) {
            Bundle = bundle;

            return true;
        }

        Bundle = null;
        StoryboardManager.Instance.Logger.LogWarning($"Failed to load AssetBundle {bundleName}");

        return false;
    }

    public override void Unload() {
        StoryboardManager.Instance.AssetBundleManager.UnloadAssetBundle(bundleName);
        Bundle = null;
    }
}
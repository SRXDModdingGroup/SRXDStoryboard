using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedAssetBundleReference : LoadedObjectReference {

    public override object LoadedObject => Bundle;
    
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;
    private IAssetBundleManager assetBundleManager;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public bool TryLoad(IAssetBundleManager assetBundleManager, ILogger logger) {
        if (assetBundleManager.TryGetAssetBundle(bundleName, out var bundle)) {
            Bundle = bundle;
            this.assetBundleManager = assetBundleManager;

            return true;
        }

        Bundle = null;
        this.assetBundleManager = null;
        logger.LogWarning($"Failed to load AssetBundle {bundleName}");

        return false;
    }

    public void Unload() {
        assetBundleManager?.UnloadAssetBundle(bundleName);
        Bundle = null;
        assetBundleManager = null;
    }
}
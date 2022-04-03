using SMU;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class AssetBundleManager : IAssetBundleManager {
    private string customAssetBundlePath;
    
    public AssetBundleManager(string customAssetBundlePath) => this.customAssetBundlePath = customAssetBundlePath;

    public void UnloadAssetBundle(string bundleName) => AssetBundleUtility.UnloadAssetBundle(customAssetBundlePath, bundleName);

    public bool TryGetAssetBundle(string bundleName, out AssetBundle bundle)
        => AssetBundleUtility.TryGetAssetBundle(customAssetBundlePath, bundleName, out bundle);
}
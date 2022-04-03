using UnityEngine;

namespace StoryboardSystem; 

public interface IAssetBundleManager {
    void UnloadAssetBundle(string bundleName);
    
    bool TryGetAssetBundle(string bundleName, out AssetBundle bundle);
}
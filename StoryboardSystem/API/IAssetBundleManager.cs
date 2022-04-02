using UnityEngine;

namespace StoryboardSystem; 

public interface IAssetBundleManager {
    bool TryGetAssetBundle(string bundleName, out AssetBundle bundle);
}
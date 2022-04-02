using UnityEngine;

namespace StoryboardSystem.Core; 

public interface IAssetBundleManager {
    bool TryGetAssetBundle(string bundleName, out AssetBundle bundle);
}
using UnityEngine;

namespace SRXDStoryboard.Core; 

public interface IAssetBundleManager {
    bool TryGetAssetBundle(string bundleName, out AssetBundle bundle);
}
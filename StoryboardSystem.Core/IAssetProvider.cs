using UnityEngine;

namespace StoryboardSystem.Core; 

public interface IAssetProvider {
    public bool TryGetAsset(string assetBundleName, string assetName, out Object asset);
}
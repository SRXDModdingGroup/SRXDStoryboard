using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal class LoadedAssetReference : LoadedObjectReference {
    public override object LoadedObject => Asset;
    
    public string AssetName { get; }
    
    public Object Asset { get; private set; }

    private int assetBundleReferenceIndex;

    public LoadedAssetReference(int assetBundleReferenceIndex, string assetName) {
        this.assetBundleReferenceIndex = assetBundleReferenceIndex;
        AssetName = assetName;
    }

    public override void Serialize(BinaryWriter writer) {
        writer.Write(assetBundleReferenceIndex);
        writer.Write(AssetName);
    }

    public override void Unload(ISceneManager sceneManager) => Asset = null;

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (assetBundleReferenceIndex < 0 || assetBundleReferenceIndex >= objectReferences.Count) {
            logger.LogWarning($"Reference index for {AssetName} is not valid");

            return false;
        }

        if (objectReferences[assetBundleReferenceIndex] is not LoadedAssetBundleReference assetBundleReference) {
            logger.LogWarning($"{objectReferences[assetBundleReferenceIndex]} is not an asset bundle");

            return false;
        }
        
        Asset = assetBundleReference.Bundle.LoadAsset(AssetName);

        if (Asset != null)
            return true;
        
        logger.LogWarning($"Failed to load asset {AssetName}");

        return false;
    }
}
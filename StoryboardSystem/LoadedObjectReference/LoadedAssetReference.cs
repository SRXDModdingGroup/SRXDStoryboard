﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal class LoadedAssetReference : LoadedObjectReference {
    public override object LoadedObject => asset;
    
    private Identifier assetBundleReference;
    private string assetName;
    private Object asset;

    public LoadedAssetReference(Identifier assetBundleReference, string assetName) {
        this.assetBundleReference = assetBundleReference;
        this.assetName = assetName;
    }

    public override void Serialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.Asset);
        assetBundleReference.Serialize(writer);
        writer.Write(assetName);
    }

    public override void Unload(ISceneManager sceneManager) => asset = null;

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (!Binder.TryResolveIdentifier(assetBundleReference, objectReferences, logger, out object obj))
            return false;

        if (obj is not AssetBundle assetBundle) {
            logger.LogWarning($"{assetBundleReference} is not an asset bundle");

            return false;
        }
        
        asset = assetBundle.LoadAsset(assetName);

        if (asset != null)
            return true;
        
        logger.LogWarning($"Failed to load asset {assetName}");

        return false;
    }

    public static LoadedAssetReference Deserialize(BinaryReader reader) {
        var assetBundleReference = Identifier.Deserialize(reader);
        string assetName = reader.ReadString();

        return new LoadedAssetReference(assetBundleReference, assetName);
    }
}
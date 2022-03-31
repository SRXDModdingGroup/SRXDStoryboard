﻿using UnityEngine;

namespace StoryboardSystem.Core; 

internal class LoadedAssetBundleReference : LoadedObjectReference {
    public AssetBundle Bundle { get; private set; }
    
    private string bundleName;

    public LoadedAssetBundleReference(string bundleName) => this.bundleName = bundleName;

    public override void Load() {
        if (StoryboardManager.AssetBundleManager.TryGetAssetBundle(bundleName, out var bundle))
            Bundle = bundle;
        else
            Bundle = null;
    }

    public override void Unload() {
        Bundle.Unload(false);
        Bundle = null;
    }
}
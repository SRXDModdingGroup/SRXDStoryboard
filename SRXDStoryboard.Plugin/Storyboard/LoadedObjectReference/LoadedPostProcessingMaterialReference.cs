using SRXDPostProcessing;
using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class LoadedPostProcessingMaterialReference : LoadedObjectReference {
    private LoadedAssetReference<Material> materialReference;
    private PostProcessingLayer layer;
    private PostProcessingInstance instance;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> materialReference, PostProcessingLayer layer) {
        this.materialReference = materialReference;
        this.layer = layer;
    }

    public override void Load() {
        instance = new PostProcessingInstance(materialReference.Asset, layer: layer);
        PostProcessingManager.AddPostProcessingInstance(instance);
    }

    public override void Unload() {
        PostProcessingManager.RemovePostProcessingInstance(instance);
        instance = null;
    }
}
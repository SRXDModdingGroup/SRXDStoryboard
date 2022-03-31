using UnityEngine;

namespace StoryboardSystem.Core; 

public class LoadedPostProcessingMaterialReference : LoadedObjectReference {
    private LoadedAssetReference<Material> materialReference;
    private int layer;
    private PostProcessingInfo info;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> materialReference, int layer) {
        this.materialReference = materialReference;
        this.layer = layer;
    }

    public override void Load() {
        info = new PostProcessingInfo(materialReference.Asset, layer);
        StoryboardManager.PostProcessingManager.AddPostProcessingInstance(info);
    }

    public override void Unload() {
        StoryboardManager.PostProcessingManager.RemovePostProcessingInstance(info);
        info = default;
    }
}
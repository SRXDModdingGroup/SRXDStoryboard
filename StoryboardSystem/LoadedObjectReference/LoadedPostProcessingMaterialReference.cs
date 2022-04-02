using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedObjectReference {
    public override object LoadedObject => info;

    private LoadedAssetReference<Material> materialReference;
    private int layer;
    private PostProcessingInfo info;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> materialReference, int layer) {
        this.materialReference = materialReference;
        this.layer = layer;
    }

    public override void Load() {
        info = new PostProcessingInfo(materialReference.Asset, layer);
        StoryboardManager.Instance.PostProcessingManager.AddPostProcessingInstance(info);
    }

    public override void Unload() {
        StoryboardManager.Instance.PostProcessingManager.RemovePostProcessingInstance(info);
        info = default;
    }
}
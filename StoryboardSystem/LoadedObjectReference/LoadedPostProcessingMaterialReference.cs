using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedObjectReference {
    public override object LoadedObject => info;

    private LoadedInstanceReference<Material> materialReference;
    private int layer;
    private PostProcessingInfo info;

    public LoadedPostProcessingMaterialReference(LoadedInstanceReference<Material> materialReference, int layer) {
        this.materialReference = materialReference;
        this.layer = layer;
    }

    public override void Load() => info = new PostProcessingInfo(materialReference.Instance, layer);

    public override void Unload() {
        StoryboardManager.Instance.PostProcessingManager.RemovePostProcessingInstance(info);
        info = default;
    }

    public void SetEnabled(bool enabled) {
        if (enabled)
            StoryboardManager.Instance.PostProcessingManager.AddPostProcessingInstance(info);
        else
            StoryboardManager.Instance.PostProcessingManager.RemovePostProcessingInstance(info);
    }
}
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

    public override bool TryLoad() {
        if (materialReference.Instance == null) {
            StoryboardManager.Instance.Logger.LogWarning("Failed to create post processing instance");
            
            return false;
        }
        
        info = new PostProcessingInfo(materialReference.Instance, layer);

        return true;
    }

    public override void Unload() {
        if (info.Material == null)
            return;
        
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
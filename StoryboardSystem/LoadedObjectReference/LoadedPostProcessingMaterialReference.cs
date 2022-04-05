using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedInstanceReference<Material> {
    public override object LoadedObject => info;

    private bool enabled = true;
    public bool Enabled {
        get => enabled;
        set {
            enabled = value;
            UpdateEnabled();
        }
    }

    private bool storyboardEnabled;
    private int layer;
    private PostProcessingInfo info;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, string name, int layer) : base(template, name) => this.layer = layer;

    public override bool TryLoad() {
        if (!base.TryLoad())
            return false;
            
        info = new PostProcessingInfo(Instance, layer);
        StoryboardManager.Instance.PostProcessingManager.AddPostProcessingInstance(info);

        return true;
    }

    public override void Unload() {
        if (info.Material == null)
            return;
        
        StoryboardManager.Instance.PostProcessingManager.RemovePostProcessingInstance(info);
        info = default;
        base.Unload();
    }

    public void SetStoryboardEnabled(bool storyboardEnabled) {
        this.storyboardEnabled = storyboardEnabled;
        UpdateEnabled();
    }

    private void UpdateEnabled() => StoryboardManager.Instance.PostProcessingManager.SetPostProcessingInstanceEnabled(info, enabled && storyboardEnabled);
}
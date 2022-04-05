using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedInstanceReference<Material> {
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

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, string name, int layer) : base(template, name) => this.layer = layer;

    public override bool TryLoad() {
        if (!base.TryLoad())
            return false;
        
        StoryboardManager.Instance.PostProcessingManager.AddPostProcessingInstance(Instance, layer);

        return true;
    }

    public override void Unload() {
        if (Instance == null)
            return;
        
        StoryboardManager.Instance.PostProcessingManager.RemovePostProcessingInstance(Instance);
        base.Unload();
    }

    public void SetStoryboardEnabled(bool storyboardEnabled) {
        this.storyboardEnabled = storyboardEnabled;
        UpdateEnabled();
    }

    private void UpdateEnabled() => StoryboardManager.Instance.PostProcessingManager.SetPostProcessingInstanceEnabled(Instance, enabled && storyboardEnabled);
}
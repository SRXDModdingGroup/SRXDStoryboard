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

    private bool storyboardActive;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, string name, int layer) : base(template, name, layer) { }

    public override void Unload() {
        if (Instance == null)
            return;
        
        StoryboardManager.Instance.SceneManager.RemovePostProcessingInstance(Instance);
        base.Unload();
    }

    public void SetStoryboardActive(bool storyboardActive) {
        this.storyboardActive = storyboardActive;
        UpdateEnabled();
    }

    public override bool TryLoad() {
        if (!base.TryLoad())
            return false;
        
        StoryboardManager.Instance.SceneManager.AddPostProcessingInstance(Instance, Layer);

        return true;
    }

    private void UpdateEnabled() => StoryboardManager.Instance.SceneManager.SetPostProcessingInstanceEnabled(Instance, enabled && storyboardActive);
}
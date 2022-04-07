using System;
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
    private ISceneManager sceneManager;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, string name, int layer) : base(template, name, layer) { }

    public override void Unload() {
        if (Instance != null && sceneManager != null)
            sceneManager.RemovePostProcessingInstance(Instance);
        
        sceneManager = null;
        base.Unload();
    }

    public void SetStoryboardActive(bool storyboardActive) {
        this.storyboardActive = storyboardActive;
        UpdateEnabled();
    }

    public override bool TryLoad(ISceneManager sceneManager, ILogger logger) {
        if (!base.TryLoad(sceneManager, logger))
            return false;

        this.sceneManager = sceneManager;
        sceneManager.AddPostProcessingInstance(Instance, Layer);

        return true;
    }

    private void UpdateEnabled() => sceneManager.SetPostProcessingInstanceEnabled(Instance, enabled && storyboardActive);
}
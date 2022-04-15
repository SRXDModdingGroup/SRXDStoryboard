using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingReference : LoadedObjectReference {
    public override object LoadedObject => instance;
    
    private bool enabled = true;
    public bool Enabled {
        get => enabled;
        set {
            enabled = value;
            UpdateEnabled();
        }
    }

    private bool storyboardActive;
    private LoadedAssetReference<Material> template;
    private Identifier targetCameraIdentifier;
    private ISceneManager sceneManager;
    private Camera targetCamera;
    private Material instance;

    public LoadedPostProcessingReference(LoadedAssetReference<Material> template, Identifier targetCameraIdentifier) {
        this.template = template;
        this.targetCameraIdentifier = targetCameraIdentifier;
    }

    public void Unload() {
        if (instance != null && sceneManager != null)
            sceneManager.RemovePostProcessingInstance(instance, targetCamera);
        
        if (instance != null)
            Object.Destroy(instance);
        
        sceneManager = null;
        instance = null;
        targetCamera = null;
    }

    public void SetStoryboardActive(bool storyboardActive) {
        this.storyboardActive = storyboardActive;
        UpdateEnabled();
    }

    public bool TryLoad(ISceneManager sceneManager, ILogger logger) {
        if (template.Asset == null) {
            logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }

        if (!Binder.TryResolveIdentifier(targetCameraIdentifier, out object result)) {
            logger.LogWarning($"Could not resolve identifier {targetCameraIdentifier}");

            return false;
        }

        if (result is not Camera camera) {
            logger.LogWarning($"{targetCameraIdentifier} is not a camera");

            return false;
        }

        targetCamera = camera;
        instance = Object.Instantiate(template.Asset);
        this.sceneManager = sceneManager;
        sceneManager.AddPostProcessingInstance(instance, camera);

        return true;
    }

    private void UpdateEnabled() => sceneManager.SetPostProcessingInstanceEnabled(instance, targetCamera, enabled && storyboardActive);
}
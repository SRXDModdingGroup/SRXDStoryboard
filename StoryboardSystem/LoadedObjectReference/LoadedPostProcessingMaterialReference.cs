using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedObjectReference {
    public override object LoadedObject { get; }
    
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
    private int targetCameraIndex;
    private ISceneManager sceneManager;
    private Camera targetCamera;
    private Material instance;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, int targetCameraIndex) {
        this.template = template;
        this.targetCameraIndex = targetCameraIndex;
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

    public bool TryLoad(Camera[] cameras, ISceneManager sceneManager, ILogger logger) {
        if (template.Asset == null) {
            logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }

        if (targetCameraIndex < 0 || targetCameraIndex >= cameras.Length) {
            logger.LogWarning($"{targetCameraIndex} is not a valid camera index");

            return false;
        }

        targetCamera = cameras[targetCameraIndex];
        instance = Object.Instantiate(template.Asset);
        this.sceneManager = sceneManager;
        sceneManager.AddPostProcessingInstance(instance, targetCamera);

        return true;
    }

    private void UpdateEnabled() => sceneManager.SetPostProcessingInstanceEnabled(instance, targetCamera, enabled && storyboardActive);
}
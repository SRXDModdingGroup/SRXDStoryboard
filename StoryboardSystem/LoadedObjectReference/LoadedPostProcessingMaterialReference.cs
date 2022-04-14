using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingMaterialReference : LoadedObjectReference {
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
    private object targetCameraReference;
    private ISceneManager sceneManager;
    private Camera targetCamera;
    private Material instance;

    public LoadedPostProcessingMaterialReference(LoadedAssetReference<Material> template, object targetCameraReference) {
        this.template = template;
        this.targetCameraReference = targetCameraReference;
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

        switch (targetCameraReference) {
            case Identifier identifier when Binder.TryResolveIdentifier(identifier, out object result): {
                if (result is not Camera camera) {
                    logger.LogWarning($"{identifier} is not a camera");

                    return false;
                }

                targetCamera = camera;
                
                break;
            }
            case LoadedExternalObjectReference reference: {
                if (reference.LoadedObject is not Camera camera) {
                    logger.LogWarning($"{reference.LoadedObject} is not a camera");

                    return false;
                }

                targetCamera = camera;
                
                break;
            }
            default:
                return false;
        }

        instance = Object.Instantiate(template.Asset);
        this.sceneManager = sceneManager;
        sceneManager.AddPostProcessingInstance(instance, targetCamera);

        return true;
    }

    private void UpdateEnabled() => sceneManager.SetPostProcessingInstanceEnabled(instance, targetCamera, enabled && storyboardActive);
}
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingReference : LoadedObjectReference {
    public override object LoadedObject => instance;

    private int assetReferenceIndex;
    private Identifier targetCameraIdentifier;
    private ISceneManager sceneManager;
    private Camera targetCamera;
    private Material instance;

    public LoadedPostProcessingReference(int assetReferenceIndex, Identifier targetCameraIdentifier) {
        this.assetReferenceIndex = assetReferenceIndex;
        this.targetCameraIdentifier = targetCameraIdentifier;
    }
    
    public void SetEnabled(bool enabled) => sceneManager.SetPostProcessingInstanceEnabled(instance, targetCamera, enabled);

    public override void Serialize(BinaryWriter writer) {
        writer.Write(assetReferenceIndex);
        targetCameraIdentifier.Serialize(writer);
    }

    public override void Unload(ISceneManager sceneManager) {
        if (instance != null && sceneManager != null)
            sceneManager.RemovePostProcessingInstance(instance, targetCamera);
        
        if (instance != null)
            Object.Destroy(instance);
        
        sceneManager = null;
        instance = null;
        targetCamera = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (assetReferenceIndex < 0 || assetReferenceIndex >= objectReferences.Count) {
            logger.LogWarning($"Reference index is not valid");

            return false;
        }
        
        if (objectReferences[assetReferenceIndex] is not LoadedAssetReference assetReference) {
            logger.LogWarning($"{objectReferences[assetReferenceIndex]} is not an asset reference");

            return false;
        }
        
        if (assetReference.Asset == null) {
            logger.LogWarning($"Failed to create instance of {assetReference.AssetName}");
            
            return false;
        }
        
        if (assetReference.Asset is not Material material) {
            logger.LogWarning($"{assetReference.AssetName} is not a material");
            
            return false;
        }

        if (!Binder.TryResolveIdentifier(targetCameraIdentifier, objectReferences, logger, out object result)) {
            logger.LogWarning($"Could not resolve identifier {targetCameraIdentifier}");

            return false;
        }

        if (result is not Camera camera) {
            logger.LogWarning($"{targetCameraIdentifier} is not a camera");

            return false;
        }

        targetCamera = camera;
        instance = Object.Instantiate(material);
        this.sceneManager = sceneManager;
        sceneManager.AddPostProcessingInstance(instance, camera);

        return true;
    }
}
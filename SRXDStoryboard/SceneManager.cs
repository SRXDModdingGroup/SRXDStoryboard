using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class SceneManager : ISceneManager {
    private Transform foregroundRoot;
    private Transform backgroundRoot;
    private Transform trackRoot;
    private Dictionary<int, PostProcessingInstance> postProcessingInfos = new();

    public int LayerCount => 3;

    public void Update(float time, bool triggerEvents) {
        var target = Patches.TrackTextureCameraTransform;
            
        if (target == null || trackRoot == null)
            return;

        trackRoot.position = target.position;
        trackRoot.rotation = target.rotation;
    }

    public void InitializeObject(Object uObject, int layer) {
        if (uObject is not GameObject gameObject)
            return;

        Layers.Layer renderLayer;
        
        switch (layer) {
            case 0:
                renderLayer = Layers.Default;
                break;
            case 1:
                renderLayer = Layers.Background;
                break;
            case 2:
                renderLayer = Layers.TrackTexture;
                break;
            default:
                return;
        }
        
        renderLayer.ApplyToObject(gameObject);
    }

    public void AddPostProcessingInstance(Material material, int layer) {
        if (postProcessingInfos.ContainsKey(material.GetInstanceID()))
            return;

        var instance = new PostProcessingInstance(material, true, (PostProcessingLayer) layer);
        
        PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInfos.Add(material.GetInstanceID(), instance);
    }

    public void RemovePostProcessingInstance(Material material) {
        if (!postProcessingInfos.TryGetValue(material.GetInstanceID(), out var instance))
            return;
        
        PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInfos.Remove(material.GetInstanceID());
    }

    public void SetPostProcessingInstanceEnabled(Material material, bool enabled) {
        if (postProcessingInfos.TryGetValue(material.GetInstanceID(), out var instance))
            instance.Enabled = enabled;
    }

    public Transform GetLayerRoot(int index) {
        switch (index) {
            case 1:
                if (backgroundRoot != null)
                    return backgroundRoot;
                
                backgroundRoot = new GameObject("BackgroundRoot").transform;
                backgroundRoot.SetParent(MainCamera.Instance.backgroundCamera.transform, false);
                backgroundRoot.localPosition = Vector3.zero;
                backgroundRoot.localRotation = Quaternion.identity;
                backgroundRoot.localScale = Vector3.one;

                return backgroundRoot;
            case 2:
                if (trackRoot != null)
                    return trackRoot;

                trackRoot = new GameObject("TrackRoot").transform;
                trackRoot.SetParent(null, false);
                trackRoot.localScale = Vector3.one;

                return trackRoot;
        }
        
        if (foregroundRoot != null)
            return foregroundRoot;
                
        foregroundRoot = new GameObject("ForegroundRoot").transform;
        foregroundRoot.SetParent(MainCamera.Instance.trackCamera.transform, false);
        foregroundRoot.localPosition = Vector3.zero;
        foregroundRoot.localRotation = Quaternion.identity;
        foregroundRoot.localScale = Vector3.one;

        return foregroundRoot;
    }
}
using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class SceneManager : ISceneManager {
    private Transform foregroundRoot;
    private Transform backgroundRoot;
    private Dictionary<int, PostProcessingInstance> postProcessingInfos = new();

    public int LayerCount => 2;

    public SceneManager(Transform foregroundRoot, Transform backgroundRoot) {
        this.foregroundRoot = foregroundRoot;
        this.backgroundRoot = backgroundRoot;
    }

    public void Update(float time, bool triggerEvents) { }

    public void InitializeObject(Object uObject, int layer) {
        if (uObject is not GameObject gameObject)
            return;

        var renderLayer = layer switch {
            1 => Layers.Background,
            _ => Layers.Default
        };

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

    public Transform GetLayerRoot(int index) => index switch {
        1 => backgroundRoot,
        _ => foregroundRoot
    };
}
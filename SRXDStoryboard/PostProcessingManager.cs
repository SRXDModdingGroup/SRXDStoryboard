using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class PostProcessingManager : IPostProcessingManager {
    private Dictionary<int, PostProcessingInstance> postProcessingInfos = new();

    public void AddPostProcessingInstance(Material material, int layer) {
        if (postProcessingInfos.ContainsKey(material.GetInstanceID()))
            return;

        var instance = new PostProcessingInstance(material, true, (PostProcessingLayer) layer);
        
        SRXDPostProcessing.PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInfos.Add(material.GetInstanceID(), instance);
    }

    public void RemovePostProcessingInstance(Material material) {
        if (!postProcessingInfos.TryGetValue(material.GetInstanceID(), out var instance))
            return;
        
        SRXDPostProcessing.PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInfos.Remove(material.GetInstanceID());
    }

    public void SetPostProcessingInstanceEnabled(Material material, bool enabled) {
        if (postProcessingInfos.TryGetValue(material.GetInstanceID(), out var instance))
            instance.Enabled = enabled;
    }
}
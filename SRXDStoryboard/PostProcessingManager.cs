using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class PostProcessingManager : IPostProcessingManager {
    private Dictionary<Material, PostProcessingInstance> postProcessingInfos = new();

    public void AddPostProcessingInstance(PostProcessingInfo info) {
        if (postProcessingInfos.ContainsKey(info.Material))
            return;

        var instance = new PostProcessingInstance(info.Material, true, (PostProcessingLayer) info.Layer);
        
        SRXDPostProcessing.PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInfos.Add(info.Material, instance);
    }

    public void RemovePostProcessingInstance(PostProcessingInfo info) {
        if (!postProcessingInfos.TryGetValue(info.Material, out var instance))
            return;
        
        SRXDPostProcessing.PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInfos.Remove(info.Material);
    }
}
using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class PostProcessingManager : IPostProcessingManager {
    private Dictionary<int, PostProcessingInstance> postProcessingInfos = new();

    public void AddPostProcessingInstance(PostProcessingInfo info) {
        if (postProcessingInfos.ContainsKey(info.Material.GetInstanceID()))
            return;

        var instance = new PostProcessingInstance(info.Material, true, (PostProcessingLayer) info.Layer);
        
        SRXDPostProcessing.PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInfos.Add(info.Material.GetInstanceID(), instance);
        Plugin.Logger.LogMessage(postProcessingInfos.Count);
    }

    public void RemovePostProcessingInstance(PostProcessingInfo info) {
        if (!postProcessingInfos.TryGetValue(info.Material.GetInstanceID(), out var instance))
            return;
        
        SRXDPostProcessing.PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInfos.Remove(info.Material.GetInstanceID());
    }
}
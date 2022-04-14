using System.Collections.Generic;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class SceneManager : ISceneManager {
    public IReadOnlyList<Transform> SceneRoots { get; }
    
    public IReadOnlyList<Camera> Cameras { get; }

    private Dictionary<PostProcessingInfo, PostProcessingInstance> postProcessingInstances = new();

    public SceneManager(Transform mainCameraRoot, Camera mainCamera, Camera backgroundCamera) {
        SceneRoots = new[] { mainCameraRoot };
        Cameras = new[] { mainCamera, backgroundCamera };
    }

    public void Update(float time, bool triggerEvents) { }

    public void InitializeObject(Object uObject) { }

    public void AddPostProcessingInstance(Material material, Camera targetCamera) {
        var info = new PostProcessingInfo(material, targetCamera);
        
        if (postProcessingInstances.ContainsKey(info))
            return;

        var instance = new PostProcessingInstance(material, targetCamera);
        
        PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInstances.Add(info, instance);
    }

    public void RemovePostProcessingInstance(Material material, Camera targetCamera) {
        var info = new PostProcessingInfo(material, targetCamera);
        
        if (!postProcessingInstances.TryGetValue(info, out var instance))
            return;
        
        PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInstances.Remove(info);
    }

    public void SetPostProcessingInstanceEnabled(Material material, Camera targetCamera, bool enabled) {
        if (postProcessingInstances.TryGetValue(new PostProcessingInfo(material, targetCamera), out var instance))
            instance.Enabled = enabled;
    }
}
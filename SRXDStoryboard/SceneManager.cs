using System.Collections.Generic;
using SMU;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class SceneManager : ISceneManager {
    private string customAssetBundlePath;
    private Dictionary<PostProcessingInfo, PostProcessingInstance> postProcessingInstances = new();

    public SceneManager(string customAssetBundlePath) {
        this.customAssetBundlePath = customAssetBundlePath;
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

    public void UnloadAssetBundle(string bundleName) => AssetBundleUtility.UnloadAssetBundle(customAssetBundlePath, bundleName);

    public bool TryGetAssetBundle(string bundleName, out AssetBundle bundle)
        => AssetBundleUtility.TryGetAssetBundle(customAssetBundlePath, bundleName, out bundle);
}
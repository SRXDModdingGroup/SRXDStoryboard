using UnityEngine;

namespace StoryboardSystem; 

public interface ISceneManager {
    void Update(float time, bool triggerEvents);

    void Start(Storyboard storyboard);

    void Stop(Storyboard storyboard);
    
    void AddPostProcessingInstance(Material material, Camera targetCamera);
    
    void RemovePostProcessingInstance(Material material, Camera targetCamera);

    void SetPostProcessingInstanceEnabled(Material material, Camera targetCamera, bool enabled);
    
    void UnloadAssetBundle(string bundleName);
    
    bool TryGetAssetBundle(string bundleName, out AssetBundle bundle);
    
    bool TryGetExternalObject(string name, out object externalObject);
    
    float Convert(float beats, float ticks, float seconds);
}
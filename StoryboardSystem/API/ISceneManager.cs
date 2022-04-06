using UnityEngine;

namespace StoryboardSystem; 

public interface ISceneManager {
    int LayerCount { get; }

    void Update(float time, bool triggerEvents);
    
    void InitializeObject(Object uObject, int layer);
    
    void AddPostProcessingInstance(Material material, int layer);
    
    void RemovePostProcessingInstance(Material material);

    void SetPostProcessingInstanceEnabled(Material material, bool enabled);

    Transform GetLayerRoot(int index);
}
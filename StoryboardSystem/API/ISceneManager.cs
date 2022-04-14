using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

public interface ISceneManager {
    IReadOnlyList<Transform> SceneRoots { get; }
    
    IReadOnlyList<Camera> Cameras { get; }

    void Update(float time, bool triggerEvents);
    
    void InitializeObject(Object uObject);
    
    void AddPostProcessingInstance(Material material, Camera targetCamera);
    
    void RemovePostProcessingInstance(Material material, Camera targetCamera);

    void SetPostProcessingInstanceEnabled(Material material, Camera targetCamera, bool enabled);
}
using UnityEngine;

namespace StoryboardSystem; 

public interface IPostProcessingManager {
    void AddPostProcessingInstance(Material material, int layer);
    
    void RemovePostProcessingInstance(Material material);

    void SetPostProcessingInstanceEnabled(Material material, bool enabled);
}
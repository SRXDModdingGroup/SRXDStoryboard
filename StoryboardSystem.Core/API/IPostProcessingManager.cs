namespace StoryboardSystem.Core; 

public interface IPostProcessingManager {
    void AddPostProcessingInstance(PostProcessingInfo info);
    
    void RemovePostProcessingInstance(PostProcessingInfo info);
}
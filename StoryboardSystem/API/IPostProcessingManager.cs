namespace StoryboardSystem; 

public interface IPostProcessingManager {
    void AddPostProcessingInstance(PostProcessingInfo info);
    
    void RemovePostProcessingInstance(PostProcessingInfo info);

    void SetPostProcessingInstanceEnabled(PostProcessingInfo info, bool enabled);
}
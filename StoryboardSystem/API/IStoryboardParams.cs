namespace StoryboardSystem; 

public interface IStoryboardParams {
    float Convert(float measures, float beats, float ticks, float seconds);

    object GetExternalObject(string name);
}
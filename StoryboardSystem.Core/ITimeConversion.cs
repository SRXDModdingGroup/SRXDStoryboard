namespace StoryboardSystem.Core; 

public interface ITimeConversion {
    float Convert(int beats, float ticks, float seconds);
}
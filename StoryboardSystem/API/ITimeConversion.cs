namespace StoryboardSystem; 

public interface ITimeConversion {
    float Convert(int measures, int beats, float ticks, float seconds);
}
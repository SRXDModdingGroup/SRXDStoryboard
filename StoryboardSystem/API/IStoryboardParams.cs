using UnityEngine;

namespace StoryboardSystem; 

public interface IStoryboardParams {
    float Convert(int measures, int beats, float ticks, float seconds);

    Object GetExternalObject(string name);
}
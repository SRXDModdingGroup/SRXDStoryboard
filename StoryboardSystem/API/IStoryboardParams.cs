﻿using UnityEngine;

namespace StoryboardSystem; 

public interface IStoryboardParams {
    float Convert(float measures, float beats, float ticks, float seconds);

    Object GetExternalObject(string name);
}
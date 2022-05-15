using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class PropertyLaneGroup {
    public List<ArrayKeyframe> ArrayLane { get; }

    public List<Keyframe>[] SubLanes { get; }
    
    public PropertyLaneGroup(int subLaneCount) {
        ArrayLane = new List<ArrayKeyframe>();
        SubLanes = new List<Keyframe>[subLaneCount];

        for (int i = 0; i < subLaneCount; i++)
            SubLanes[i] = new List<Keyframe>();
    }
}
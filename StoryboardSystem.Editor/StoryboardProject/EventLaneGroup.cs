using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class EventLaneGroup {
    public List<ArrayEventFrame> ArrayLane { get; }
    
    public List<EventFrame>[] SubLanes { get; }

    public EventLaneGroup(int subLaneCount) {
        ArrayLane = new List<ArrayEventFrame>();
        SubLanes = new List<EventFrame>[subLaneCount];

        for (int i = 0; i < subLaneCount; i++)
            SubLanes[i] = new List<EventFrame>();
    }
}
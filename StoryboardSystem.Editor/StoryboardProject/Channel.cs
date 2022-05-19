using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class Channel {
    public List<EventFrame>[] EventLanes { get; }
    
    public List<PropertyFrame>[] PropertyLanes { get; }

    public Channel(RigSetup setup) {
        EventLanes = new List<EventFrame>[setup.Events.Length];
        PropertyLanes = new List<PropertyFrame>[setup.Properties.Length];

        for (int i = 0; i < EventLanes.Length; i++)
            EventLanes[i] = new List<EventFrame>();

        for (int i = 0; i < PropertyLanes.Length; i++)
            PropertyLanes[i] = new List<PropertyFrame>();
    }
}
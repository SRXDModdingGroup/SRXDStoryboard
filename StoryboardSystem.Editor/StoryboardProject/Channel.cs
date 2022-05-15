using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class Channel {
    public EventLaneGroup[] EventLaneGroups { get; }
    
    public PropertyLaneGroup[] PropertyLaneGroups { get; }

    public Channel(RigSettings settings) {
        EventLaneGroups = new EventLaneGroup[settings.events.Length];
        PropertyLaneGroups = new PropertyLaneGroup[settings.properties.Length];
        
        int subRigCount = settings.count;

        for (int i = 0; i < EventLaneGroups.Length; i++)
            EventLaneGroups[i] = new EventLaneGroup(subRigCount);

        for (int i = 0; i < PropertyLaneGroups.Length; i++)
            PropertyLaneGroups[i] = new PropertyLaneGroup(subRigCount);
    }
}
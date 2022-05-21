using System.Collections.Generic;

namespace StoryboardSystem.Editor;

public class Lane {
    public int RigIndex { get; }
    
    public int PropertyIndex { get; }

    public LaneType Type { get; }
    
    public LaneData Data { get; set; }

    public List<Frame> Frames { get; }

    public Lane(int rigIndex, int propertyIndex, LaneType type, LaneData data) {
        RigIndex = rigIndex;
        PropertyIndex = propertyIndex;
        Type = type;
        Data = data;
        Frames = new List<Frame>();
    }
}
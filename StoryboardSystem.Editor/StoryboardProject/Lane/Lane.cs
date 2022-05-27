using System.Collections.Generic;
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor;

public class Lane {
    public int RigIndex { get; }
    
    public LaneData Data { get; set; }

    public List<Frame> Frames { get; }

    public Lane(int rigIndex, LaneData data) {
        RigIndex = rigIndex;
        Data = data;
        Frames = new List<Frame>();
    }
}
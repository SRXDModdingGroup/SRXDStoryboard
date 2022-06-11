using System.Collections.Generic;

namespace StoryboardSystem.Editor;

public class Lane {
    public List<Frame> Frames { get; }

    public Lane() => Frames = new List<Frame>();
}
using System.Collections.Generic;

namespace VisualizerSystem.Editor {
    public class Lane {
        public List<Frame> Frames { get; }

        public Lane() => Frames = new List<Frame>();
    }
}
using System.Collections.Generic;

namespace VisualizerSystem.Editor;

public class VisualizerProject {
    public ProjectSetup Setup { get; }

    public List<Lane> Lanes { get; }

    public VisualizerProject(ProjectSetup setup) {
        Setup = setup;
        Lanes = new List<Lane>();
    }
}
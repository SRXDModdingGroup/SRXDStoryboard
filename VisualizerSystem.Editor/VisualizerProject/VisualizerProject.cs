using System.Collections.Generic;

namespace VisualizerSystem.Editor;

public class VisualizerProject {
    public ProjectSetup Setup { get; }

    public List<Lane> Lanes { get; }

    public VisualizerProject(ProjectSetup setup) {
        Setup = setup;
        Lanes = new List<Lane>();

        for (int i = 0; i < setup.Rigs.Count; i++)
            Lanes.Add(new Lane());
    }
}
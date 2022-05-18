using System.Collections.Generic;

namespace StoryboardSystem.Editor;

public class StoryboardProject {
    public ProjectSetup Setup { get; }
    
    public List<Pattern> Patterns { get; }
    
    public double[] BeatArray { get; private set; }

    public StoryboardProject(ProjectSetup setup) {
        Patterns = new List<Pattern>();
        Setup = setup;
    }

    public void AssignBeatArray(double[] beatArray) => BeatArray = beatArray;
}
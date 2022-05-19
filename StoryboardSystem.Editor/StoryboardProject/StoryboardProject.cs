using System.Collections.Generic;

namespace StoryboardSystem.Editor;

public class StoryboardProject {
    public ProjectSetup Setup { get; }

    public List<Pattern> Patterns { get; }
    
    public List<PatternInstance> PatternInstances { get; }

    public StoryboardProject(ProjectSetup setup) {
        Setup = setup;
        Patterns = new List<Pattern>();
        PatternInstances = new List<PatternInstance>();
    }
}
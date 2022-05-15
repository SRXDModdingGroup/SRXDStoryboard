using System.Collections.Generic;
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor;

public class StoryboardProject {
    public RigSettings[] Rigs { get; private set; }
    
    public double[] BeatArray { get; private set; }
    
    public List<Pattern> Patterns { get; }

    public void AssignRigs(RigSettings[] rigs) => Rigs = rigs;

    public void AssignBeatArray(double[] beatArray) => BeatArray = beatArray;
}
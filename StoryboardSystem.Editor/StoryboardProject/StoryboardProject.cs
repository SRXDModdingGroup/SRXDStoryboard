using System.Collections.Generic;
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor;

public class StoryboardProject {
    public RigSetup[] Setup { get; private set; }
    
    public double[] BeatArray { get; private set; }
    
    public List<Pattern> Patterns { get; }

    public void AssignSetup(RigSetup[] setup) => Setup = setup;

    public void AssignBeatArray(double[] beatArray) => BeatArray = beatArray;
}
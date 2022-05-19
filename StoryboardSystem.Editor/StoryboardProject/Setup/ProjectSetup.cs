using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class ProjectSetup {
    public RigSetup[] Rigs { get; }
    
    public double[] BeatArray { get; }

    public ProjectSetup(RigSettings[] settings, double[] beatArray) {
        Rigs = new RigSetup[settings.Length];

        for (int i = 0; i < settings.Length; i++)
            Rigs[i] = new RigSetup(settings[i]);

        BeatArray = beatArray;
    }
}
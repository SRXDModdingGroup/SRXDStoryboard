using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class Pattern {
    public string Name { get; }
    
    public Channel[] Channels { get; }
    
    public List<PatternInstance> Instances { get; }

    public Pattern(string name, ProjectSetup setup) {
        Name = name;
        
        var rigs = setup.Rigs;
        
        Channels = new Channel[rigs.Length];

        for (int i = 0; i < rigs.Length; i++)
            Channels[i] = new Channel(rigs[i]);

        Instances = new List<PatternInstance>();
    }
}
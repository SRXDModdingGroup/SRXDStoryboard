using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class Pattern {
    public string Name { get; }
    
    public Channel[] Channels { get; }
    
    public List<PatternInstance> Instances { get; }

    public Pattern(string name, Channel[] channels, List<PatternInstance> instances) {
        Name = name;
        Channels = channels;
        Instances = instances;
    }
}
using System.Collections.Generic;
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigEventSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public RigEventType Type { get; }
    
    public List<RigParameterSetup> Parameters { get; }

    public RigEventSetup(string key, string name, RigEventType type, List<RigParameterSetup> parameters) {
        Key = key;
        Name = name;
        Type = type;
        Parameters = parameters;
    }

    public RigEventSetup(RigEventSettings settings) {
        Key = settings.key;
        Name = settings.name;
        Type = settings.type;
        Parameters = new List<RigParameterSetup>();

        foreach (var parameter in settings.parameters)
            Parameters.Add(new RigParameterSetup(parameter));
    }
}
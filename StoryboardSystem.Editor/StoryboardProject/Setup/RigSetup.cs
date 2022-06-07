using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public int Count { get; }
    
    public RigType Type { get; }
    
    public RigParameterSetup[] Parameters { get; }

    public RigSetup(string key, string name, int count, RigType type, RigParameterSetup[] parameters) {
        Key = key;
        Name = name;
        Count = count;
        Type = type;
        Parameters = parameters;
    }

    public RigSetup(RigSettings settings) {
        Key = settings.key;
        Name = settings.displayName;
        Count = settings.count;

        var definition = settings.definition;
        
        Type = definition.type;
        Parameters = new RigParameterSetup[definition.parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            Parameters[i] = new RigParameterSetup(definition.parameters[i]);
    }
}
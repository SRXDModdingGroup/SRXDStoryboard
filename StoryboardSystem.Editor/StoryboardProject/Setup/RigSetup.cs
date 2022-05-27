using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public int Count { get; }
    
    public RigType Type { get; }
    
    public RigParameterSetup[] Parameters { get; }
    
    public RigSetup(RigSettings settings) {
        Key = settings.key;
        Name = settings.name;
        Count = settings.count;
        Type = settings.type;
        Parameters = new RigParameterSetup[settings.parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            Parameters[i] = new RigParameterSetup(settings.parameters[i]);
    }
}
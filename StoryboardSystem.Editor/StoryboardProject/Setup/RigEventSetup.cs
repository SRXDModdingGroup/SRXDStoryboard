using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigEventSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public RigValueSetup[] Parameters { get; }
    
    public RigEventSetup(RigEvent settings) {
        Key = settings.key;
        Name = settings.name;
        Parameters = new RigValueSetup[settings.parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            Parameters[i] = new RigValueSetup(settings.parameters[i]);
    }
}
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigEventSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public RigEventParameterSetup[] Parameters { get; }
    
    public RigEventSetup(RigEvent settings) {
        Key = settings.key;
        Name = settings.name;
        Parameters = new RigEventParameterSetup[settings.parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            Parameters[i] = new RigEventParameterSetup(settings.parameters[i]);
    }
}
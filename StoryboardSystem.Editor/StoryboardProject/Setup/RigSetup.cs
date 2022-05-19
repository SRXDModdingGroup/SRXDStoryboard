using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public int Count { get; }
    
    public RigEventSetup[] Events { get; }
    
    public RigValueSetup[] Properties { get; }
    
    public RigSetup(RigSettings settings) {
        Key = settings.key;
        Name = settings.name;
        Count = settings.count;
        Events = new RigEventSetup[settings.events.Length];
        Properties = new RigValueSetup[settings.properties.Length];

        for (int i = 0; i < Events.Length; i++)
            Events[i] = new RigEventSetup(settings.events[i]);

        for (int i = 0; i < Properties.Length; i++)
            Properties[i] = new RigValueSetup(settings.properties[i]);
    }
}
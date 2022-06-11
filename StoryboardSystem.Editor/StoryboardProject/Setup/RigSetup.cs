using System.Collections.Generic;
using StoryboardSystem.Rigging;

namespace StoryboardSystem.Editor; 

public class RigSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public int Count { get; }
    
    public RigDefinitionSetup Definition { get; }

    public RigSetup(string key, string name, int count, RigDefinitionSetup definition) {
        Key = key;
        Name = name;
        Count = count;
        Definition = definition;
    }

    public RigSetup(RigSettings settings, RigDefinitionSetup definition) {
        Key = settings.key;
        Name = settings.displayName;
        Count = settings.count;
        Definition = definition;
    }
}
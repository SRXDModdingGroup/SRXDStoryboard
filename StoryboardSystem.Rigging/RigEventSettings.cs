using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigEventSettings {
    public string key;
    public string name;
    public RigEventType type;
    public RigParameterSettings[] parameters;
}
using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigEvent {
    public string key;
    public string name;
    public RigEventParameter[] parameters;
}
using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigEvent {
    public string name;
    public RigEventParameter[] values;
}
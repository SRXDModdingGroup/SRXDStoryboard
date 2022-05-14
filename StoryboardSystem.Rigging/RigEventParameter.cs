using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigEventParameter {
    public string name;
    public RigPropertyType type;
    public RigPropertyValue defaultValue;
    public RigPropertyValue minValue;
    public RigPropertyValue maxValue;
    public bool hardMin;
    public bool hardMax;
}
using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigProperty {
    public string key;
    public string name;
    public RigPropertyType type;
    public RigPropertyValue defaultValue;
    public RigPropertyValue minValue;
    public RigPropertyValue maxValue;
    public bool hardMin;
    public bool hardMax;
    public bool modular;
}
using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigProperty {
    public string name;
    public RiggedPropertyType type;
    public RiggedPropertyValue defaultValue;
    public RiggedPropertyValue minValue;
    public RiggedPropertyValue maxValue;
    public bool hardMin;
    public bool hardMax;
    public bool modular;
}
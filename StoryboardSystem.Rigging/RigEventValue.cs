using System;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigEventValue {
    public string name;
    public RiggedPropertyType type;
    public RiggedPropertyValue defaultValue;
    public RiggedPropertyValue minValue;
    public RiggedPropertyValue maxValue;
    public bool hardMin;
    public bool hardMax;
}
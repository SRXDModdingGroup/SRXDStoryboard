using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class RigValueSetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public RigValueType Type { get; }
    
    public Vector3 DefaultValue { get; }
    
    public Vector3 MinValue { get; }
    
    public Vector3 MaxValue { get; }
    
    public bool HasMin { get; }
    
    public bool HasMax { get; }
    
    public RigValueSetup(RigValueSettings settings) {
        Key = settings.key;
        Name = settings.name;
        Type = settings.type;
        DefaultValue = settings.defaultValue;
        MinValue = settings.minValue;
        MaxValue = settings.maxValue;
        HasMin = settings.hasMin;
        HasMax = settings.hasMax;
    }
}
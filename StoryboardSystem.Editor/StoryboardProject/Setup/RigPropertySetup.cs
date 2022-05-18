using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class RigPropertySetup {
    public string Key { get; }
    
    public string Name { get; }
    
    public RigPropertyType Type { get; }
    
    public Vector4 DefaultValue { get; }
    
    public Vector4 MinValue { get; }
    
    public Vector4 MaxValue { get; }
    
    public bool HardMin { get; }
    
    public bool HardMax { get; }
    
    public bool Modular { get; }
    
    public RigPropertySetup(RigProperty settings) {
        Key = settings.key;
        Name = settings.name;
        Type = settings.type;
        DefaultValue = CreateValueByType(settings.defaultValue, Type);
        MinValue = CreateValueByType(settings.minValue, Type);
        MaxValue = CreateValueByType(settings.maxValue, Type);
        HardMin = settings.hardMin;
        HardMax = settings.hardMax;
        Modular = settings.modular;
    }
    
    private static Vector4 CreateValueByType(RigPropertyValue value, RigPropertyType type) => type switch {
        RigPropertyType.Bool => new Vector4(value.boolVal ? 1f : 0f, 0f, 0f, 0f),
        RigPropertyType.Int => new Vector4(value.intX, 0f, 0f, 0f),
        RigPropertyType.Float => new Vector4(value.floatX, 0f, 0f, 0f),
        RigPropertyType.Vector2 => new Vector4(value.floatX, value.floatY, 0f, 0f),
        RigPropertyType.Vector3 => new Vector4(value.floatX, value.floatY, value.floatZ, 0f),
        RigPropertyType.Vector2Int => new Vector4(value.intX, value.intY, 0f, 0f),
        RigPropertyType.Vector3Int => new Vector4(value.intX, value.intY, value.intZ, 0f),
        RigPropertyType.Color => new Vector4(value.floatX, value.floatY, value.floatZ, value.floatW),
        _ => Vector4.zero
    };
}
using VisualizerSystem.Rigging;
using UnityEngine;

namespace VisualizerSystem.Editor {
    public class RigParameterSetup {
        public string Key { get; }
    
        public string Name { get; }
    
        public RigValueType Type { get; }
    
        public Vector3 DefaultValue { get; }
    
        public Vector3 MinValue { get; }
    
        public Vector3 MaxValue { get; }
    
        public bool HasMin { get; }
    
        public bool HasMax { get; }

        public RigParameterSetup(string key, string name, RigValueType type, Vector3 defaultValue, Vector3 minValue, Vector3 maxValue, bool hasMin, bool hasMax) {
            Key = key;
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            HasMin = hasMin;
            HasMax = hasMax;
        }

        public RigParameterSetup(RigParameterSettings settings) {
            Key = settings.key;
            Name = settings.name;
            Type = settings.valueType;
            DefaultValue = settings.defaultValue;
            MinValue = settings.minValue;
            MaxValue = settings.maxValue;
            HasMin = settings.hasMin;
            HasMax = settings.hasMax;
        }
    }
}
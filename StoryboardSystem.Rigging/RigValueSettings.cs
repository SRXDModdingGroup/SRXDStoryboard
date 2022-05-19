using System;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigValueSettings {
    public string key;
    public string name;
    public RigValueType type;
    public Vector3 defaultValue;
    public Vector3 minValue;
    public Vector3 maxValue;
    public bool hasMin;
    public bool hasMax;
}
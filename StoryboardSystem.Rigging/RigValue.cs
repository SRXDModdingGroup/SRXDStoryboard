using System;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

[Serializable]
public class RigValue {
    public string key;
    public string name;
    public RigValueType type;
    public Vector3 defaultValue;
    public Vector3 minValue;
    public Vector3 maxValue;
    public bool hardMin;
    public bool hardMax;
}
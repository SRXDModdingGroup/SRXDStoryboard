using System;
using UnityEngine;

namespace VisualizerSystem.Rigging {
    [Serializable]
    public class RigParameterSettings {
        public string key;
        public string name;
        public RigValueType valueType;
        public Vector3 defaultValue;
        public Vector3 minValue;
        public Vector3 maxValue;
        public bool hasMin;
        public bool hasMax;
    }
}
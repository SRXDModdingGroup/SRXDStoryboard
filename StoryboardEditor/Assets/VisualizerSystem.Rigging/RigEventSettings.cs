using System;

namespace VisualizerSystem.Rigging {
    [Serializable]
    public class RigEventSettings {
        public string key;
        public string name;
        public RigParameterSettings[] parameters;
    }
}
using System.Collections.Generic;
using VisualizerSystem.Rigging;

namespace VisualizerSystem.Editor {
    public class RigEventSetup {
        public string Key { get; }
    
        public string Name { get; }
    
        public List<RigParameterSetup> Parameters { get; }

        public RigEventSetup(string key, string name, List<RigParameterSetup> parameters) {
            Key = key;
            Name = name;
            Parameters = parameters;
        }

        public RigEventSetup(RigEventSettings settings) {
            Key = settings.key;
            Name = settings.name;
            Parameters = new List<RigParameterSetup>();

            foreach (var parameter in settings.parameters)
                Parameters.Add(new RigParameterSetup(parameter));
        }
    }
}
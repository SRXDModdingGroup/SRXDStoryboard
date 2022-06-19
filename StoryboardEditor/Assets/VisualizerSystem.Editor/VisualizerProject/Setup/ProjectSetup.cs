using System.Collections.Generic;
using VisualizerSystem.Rigging;

namespace VisualizerSystem.Editor {
    public class ProjectSetup {
        public List<RigDefinitionSetup> RigDefinitions { get; }
    
        public List<RigSetup> Rigs { get; }
    
        public double[] BeatArray { get; }

        public ProjectSetup(List<RigDefinitionSetup> rigDefinitions, List<RigSetup> rigs, double[] beatArray) {
            RigDefinitions = rigDefinitions;
            Rigs = rigs;
            BeatArray = beatArray;
        }

        public ProjectSetup(RigSettings[] rigSettings, double[] beatArray) {
            var definitionsDict = new Dictionary<RigDefinition, RigDefinitionSetup>();
        
            RigDefinitions = new List<RigDefinitionSetup>();
            Rigs = new List<RigSetup>();

            foreach (var settings in rigSettings) {
                if (!definitionsDict.TryGetValue(settings.definition, out var definition)) {
                    definition = new RigDefinitionSetup(settings.definition);
                    definitionsDict.Add(settings.definition, definition);
                }
            
                Rigs.Add(new RigSetup(settings, definition));
            }

            BeatArray = beatArray;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace VisualizerSystem.Rigging {
    public class VisualizerScene : MonoBehaviour {
        private Dictionary<string, VisualizerRig[]> rigsDict;

        public void ApplyRigs(RigSettings[] rigs) {
            rigsDict = new Dictionary<string, VisualizerRig[]>();

            foreach (var rigSettings in rigs) {
                var rigArray = new VisualizerRig[rigSettings.count];

                for (int i = 0; i < rigSettings.count; i++)
                    rigArray[i] = new VisualizerRig();
            
                rigsDict.Add(rigSettings.key, rigArray);
            }

            foreach (var rigTarget in GetComponentsInChildren<RigTarget>())
                rigTarget.Bind(this);
        }

        public bool TryGetRig(string key, int index, out VisualizerRig rig) {
            if (rigsDict != null && rigsDict.TryGetValue(key, out var rigArray) && index >= 0 && index < rigArray.Length) {
                rig = rigArray[index];

                return true;
            }

            rig = null;

            return false;
        }
    }
}
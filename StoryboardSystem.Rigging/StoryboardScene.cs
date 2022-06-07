using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class StoryboardScene : MonoBehaviour {
    private Dictionary<string, StoryboardRig[]> rigsDict;

    public void ApplyRigs(RigSettings[] rigs) {
        rigsDict = new Dictionary<string, StoryboardRig[]>();

        foreach (var rigSettings in rigs) {
            var rigArray = new StoryboardRig[rigSettings.count];

            for (int i = 0; i < rigSettings.count; i++)
                rigArray[i] = new StoryboardRig();
            
            rigsDict.Add(rigSettings.key, rigArray);
        }

        foreach (var rigTarget in GetComponentsInChildren<RigTarget>())
            rigTarget.Bind(this);
    }

    public bool TryGetRig(string key, int index, out StoryboardRig rig) {
        if (rigsDict != null && rigsDict.TryGetValue(key, out var rigArray) && index >= 0 && index < rigArray.Length) {
            rig = rigArray[index];

            return true;
        }

        rig = null;

        return false;
    }
}
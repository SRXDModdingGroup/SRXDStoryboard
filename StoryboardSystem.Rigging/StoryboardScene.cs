using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class StoryboardScene : MonoBehaviour {
    [SerializeField] private RigSettings[] rigs;

    private Dictionary<string, StoryboardRig[]> rigsDict;

    private void Awake() {
        rigsDict = new Dictionary<string, StoryboardRig[]>();

        foreach (var settings in rigs) {
            var rigArray = new StoryboardRig[settings.count];

            for (int i = 0; i < settings.count; i++)
                rigArray[i] = new StoryboardRig(settings);
            
            rigsDict.Add(settings.name, rigArray);
        }

        foreach (var rigTarget in GetComponentsInChildren<RigTarget>())
            rigTarget.Bind(this);
    }

    public bool TryGetRig(string name, int index, out StoryboardRig rig) {
        if (rigsDict.TryGetValue(name, out var rigArray) && index >= 0 && index < rigArray.Length) {
            rig = rigArray[index];

            return true;
        }

        rig = null;

        return false;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class StoryboardScene : MonoBehaviour {
    [SerializeField] private SceneSettings settings;

    private Dictionary<string, StoryboardRig[]> rigsDict;

    private void Awake() {
        rigsDict = new Dictionary<string, StoryboardRig[]>();

        foreach (var rigSettings in settings.Rigs) {
            var rigArray = new StoryboardRig[rigSettings.count];

            for (int i = 0; i < rigSettings.count; i++)
                rigArray[i] = new StoryboardRig(rigSettings);
            
            rigsDict.Add(rigSettings.key, rigArray);
        }

        foreach (var rigTarget in GetComponentsInChildren<RigTarget>())
            rigTarget.Bind(this);
    }

    public bool TryGetRig(string key, int index, out StoryboardRig rig) {
        if (rigsDict.TryGetValue(key, out var rigArray) && index >= 0 && index < rigArray.Length) {
            rig = rigArray[index];

            return true;
        }

        rig = null;

        return false;
    }
}
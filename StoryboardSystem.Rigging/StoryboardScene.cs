using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class StoryboardScene : MonoBehaviour {
    private Dictionary<string, StoryboardRig> rigs;

    public bool TryGetRig(string name, out StoryboardRig rig) => rigs.TryGetValue(name, out rig);

    private void Awake() {
        rigs = new Dictionary<string, StoryboardRig>();
        
        foreach (var rig in GetComponentsInChildren<StoryboardRig>())
            rigs.Add(rig.name, rig);
    }
}
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public abstract class RigTarget : MonoBehaviour {
    [SerializeField] private string rigName;
    [SerializeField] private int rigIndex;

    public void Bind(StoryboardScene scene) {
        if (scene.TryGetRig(rigName, rigIndex, out var rig))
            Bind(rig);
    }
    
    protected abstract void Bind(StoryboardRig rig);
}
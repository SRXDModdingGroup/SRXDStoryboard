using UnityEngine;

namespace StoryboardSystem.Rigging; 

public abstract class RigTarget : MonoBehaviour {
    [SerializeField] private string rigKey;
    [SerializeField] private int rigIndex;

    public void Bind(StoryboardScene scene) {
        if (scene.TryGetRig(rigKey, rigIndex, out var rig))
            Bind(rig);
    }
    
    protected abstract void Bind(StoryboardRig rig);
}
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public abstract class RigTarget : MonoBehaviour {
    public abstract void AssignRig(StoryboardRig rig);
}
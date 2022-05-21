using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class SceneSettings : ScriptableObject {
    [SerializeField] private RigSettings[] rigs;

    public RigSettings[] Rigs => rigs;
}
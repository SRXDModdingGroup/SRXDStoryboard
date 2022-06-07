using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class RigDefinition : ScriptableObject {
    public RigType type;
    public RigParameterSettings[] parameters;
}
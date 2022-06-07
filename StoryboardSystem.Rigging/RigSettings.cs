using System;

namespace StoryboardSystem.Rigging;

[Serializable]
public class RigSettings {
    public string key;
    public string displayName;
    public int count;
    public RigDefinition definition;
}
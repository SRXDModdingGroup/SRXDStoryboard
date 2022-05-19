using System;

namespace StoryboardSystem.Rigging;

[Serializable]
public class RigSettings {
    public string key;
    public string name;
    public int count;
    public RigEvent[] events;
    public RigValue[] properties;
}
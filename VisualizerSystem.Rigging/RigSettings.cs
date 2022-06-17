using System;

namespace VisualizerSystem.Rigging;

[Serializable]
public class RigSettings {
    public string key;
    public string displayName;
    public int count;
    public RigDefinition definition;
}
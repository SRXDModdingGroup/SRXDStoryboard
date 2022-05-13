using System.Collections.Generic;

namespace StoryboardSystem.Core;

public class StoryboardData {
    public string AssetBundleName { get; }
    
    public string ScenePrefabName { get; }
    
    public List<(string rig, string property, List<EventCall> eventCalls)> EventCalls { get; }
    
    public List<(string rig, string property, List<Curve> curves)> Curves { get; }

    public StoryboardData(string assetBundleName, string scenePrefabName, List<(string rig, string property, List<EventCall>)> eventCalls, List<(string rig, string property, List<Curve>)> curves) {
        AssetBundleName = assetBundleName;
        ScenePrefabName = scenePrefabName;
        EventCalls = eventCalls;
        Curves = curves;
    }
}

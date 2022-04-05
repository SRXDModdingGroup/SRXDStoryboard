using System.Collections.Generic;

namespace StoryboardSystem; 

internal class Storyboard {
    public string Path { get; }
    
    public ITimeConversion TimeConversion { get; }
    
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private List<TimelineBuilder> timelineBuilders;
    private Timeline[] timelines;
    private float lastTime;

    private bool loaded;

    public Storyboard(
        string path,
        ITimeConversion timeConversion,
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        List<TimelineBuilder> timelineBuilders) {
        Path = path;
        TimeConversion = timeConversion;
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.postProcessReferences = postProcessReferences;
        this.timelineBuilders = timelineBuilders;
    }

    public void Evaluate(float time, bool triggerEvents) {
        if (!loaded || time == lastTime)
            return;

        foreach (var timeline in timelines) {
            if (triggerEvents || !timeline.IsEvent)
                timeline.Evaluate(time);
        }

        lastTime = time;
    }

    public bool TryLoad(ILogger logger) {
        bool success = true;
        
        foreach (var reference in assetBundleReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in assetReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in instanceReferences)
            success = reference.TryLoad() && success;

        foreach (var reference in postProcessReferences)
            success = reference.TryLoad() && success;

        if (!success) {
            Unload();
            
            return false;
        }

        timelines = new Timeline[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateTimeline(TimeConversion, out var curve)) {
                timelines[i] = curve;
                
                continue;
            }
            
            logger.LogWarning($"Failed to create timeline {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            Unload();

            return false;
        }

        lastTime = -1f;
        loaded = true;

        return true;
    }

    public void Unload() {
        timelines = null;
        
        foreach (var reference in postProcessReferences)
            reference.Unload();
        
        foreach (var reference in instanceReferences)
            reference.Unload();
        
        foreach (var reference in assetReferences)
            reference.Unload();
        
        foreach (var reference in assetBundleReferences)
            reference.Unload();

        loaded = false;
    }

    public void SetPostProcessingEnabled(bool enabled) {
        if (!loaded)
            return;

        foreach (var reference in postProcessReferences)
            reference.SetStoryboardEnabled(enabled);
    }
}
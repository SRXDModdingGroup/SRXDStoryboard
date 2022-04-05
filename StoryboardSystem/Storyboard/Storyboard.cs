using System.Collections.Generic;

namespace StoryboardSystem; 

internal class Storyboard {
    private ITimeConversion timeConversion;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private List<TimelineBuilder> eventBuilders;
    private List<TimelineBuilder> curveBuilders;
    private Timeline[] events;
    private Timeline[] curves;
    private float lastTime;
    private bool loaded;

    public Storyboard(
        ITimeConversion timeConversion,
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        List<TimelineBuilder> eventBuilders,
        List<TimelineBuilder> curveBuilders) {
        this.timeConversion = timeConversion;
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.eventBuilders = eventBuilders;
        this.curveBuilders = curveBuilders;
        this.postProcessReferences = postProcessReferences;
    }

    public void Evaluate(float time, bool triggerEvents) {
        if (!loaded || time == lastTime)
            return;

        foreach (var curve in curves)
            curve.Evaluate(time);

        if (triggerEvents) {
            foreach (var @event in events)
                @event.Evaluate(time);
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

        curves = new Timeline[curveBuilders.Count];

        for (int i = 0; i < curveBuilders.Count; i++) {
            if (curveBuilders[i].TryCreateTimeline(timeConversion, out var curve)) {
                curves[i] = curve;
                
                continue;
            }
            
            logger.LogWarning($"Failed to create curve {curveBuilders[i].Name}");
            success = false;
        }

        events = new Timeline[eventBuilders.Count];
        
        for (int i = 0; i < eventBuilders.Count; i++) {
            if (eventBuilders[i].TryCreateTimeline(timeConversion, out var @event)) {
                events[i] = @event;
                
                continue;
            }
            
            logger.LogWarning($"Failed to create event {eventBuilders[i].Name}");
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
        events = null;
        curves = null;
        
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
            reference.SetEnabled(enabled);
    }
}
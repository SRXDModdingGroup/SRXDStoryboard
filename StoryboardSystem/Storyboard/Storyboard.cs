using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class Storyboard {
    private bool dataSet;
    private bool loaded;
    private string name;
    private string directory;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private List<TimelineBuilder> timelineBuilders;
    private Timeline[] timelines;
    private float lastTime;

    public Storyboard(
        string name,
        string directory) {
        this.name = name;
        this.directory = directory;
    }

    public void SetData(LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        List<TimelineBuilder> timelineBuilders) {
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.postProcessReferences = postProcessReferences;
        this.timelineBuilders = timelineBuilders;
        dataSet = true;
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

    public bool TryCompile(ILogger logger, bool force) {
        if (dataSet && !force)
            return true;
        
        ClearData();
        
        return Compiler.TryCompileFile(name, directory, logger, this);
    }

    public void Load(ITimeConversion timeConversion, ILogger logger) {
        if (!dataSet) {
            logger.LogWarning($"Failed to load {name}: Data is not set");

            return;
        }
        
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
            
            return;
        }

        timelines = new Timeline[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateTimeline(timeConversion, out var curve)) {
                timelines[i] = curve;
                
                continue;
            }
            
            logger.LogWarning($"Failed to load {name}: Could not create timeline {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            Unload();

            return;
        }

        lastTime = -1f;
        loaded = true;
        logger.LogMessage($"Successfully loaded {name}");
    }

    public void Unload() {
        timelines = null;
        loaded = false;
        
        if (!dataSet)
            return;
        
        foreach (var reference in postProcessReferences)
            reference.Unload();
        
        foreach (var reference in instanceReferences)
            reference.Unload();
        
        foreach (var reference in assetReferences)
            reference.Unload();
        
        foreach (var reference in assetBundleReferences)
            reference.Unload();
    }

    public void SetEnabled(bool enabled) {
        if (!loaded)
            return;

        foreach (var reference in postProcessReferences)
            reference.SetStoryboardEnabled(enabled);
    }
    
    private void ClearData() {
        Unload();
        assetBundleReferences = null;
        assetReferences = null;
        instanceReferences = null;
        postProcessReferences = null;
        timelineBuilders = null;
        dataSet = false;
    }
}
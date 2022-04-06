using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

public class Storyboard {
    public bool HasData { get; private set; }

    private bool active;
    private bool loaded;
    private float lastTime;
    private string name;
    private string directory;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private List<TimelineBuilder> timelineBuilders;
    private Dictionary<string, object> outParams;
    private Timeline[] timelines;

    internal Storyboard(
        string name,
        string directory) {
        this.name = name;
        this.directory = directory;
    }
    
    public bool TryGetOutParam<T>(string name, out T value) {
        if (outParams != null && outParams.TryGetValue(name, out object obj) && obj is T cast) {
            value = cast;

            return true;
        }

        value = default;

        return false;
    }

    internal void Play() {
        active = true;

        if (loaded) {
            foreach (var reference in postProcessReferences)
                reference.SetStoryboardActive(true);
        }
        
        Evaluate(lastTime, false);
    }

    internal void Stop() {
        active = false;

        if (!loaded)
            return;
            
        foreach (var reference in postProcessReferences)
            reference.SetStoryboardActive(false);
    }

    internal void Evaluate(float time, bool triggerEvents) {
        lastTime = time;
        
        if (!loaded || !active)
            return;

        foreach (var timeline in timelines) {
            if (triggerEvents || !timeline.IsEvent)
                timeline.Evaluate(time);
        }
    }

    internal void Compile(bool force, ILogger logger) {
        if (HasData && !force)
            return;
        
        ClearData();
        Compiler.CompileFile(name, directory, logger, this);
    }

    internal void Recompile(bool force, ITimeConversion conversion, ILogger logger) {
        if (HasData && !force)
            return;

        bool wasLoaded = loaded;
        
        Compile(force, logger);

        if (wasLoaded)
            LoadContents(conversion, logger);
    }

    internal void SetData(LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        List<TimelineBuilder> timelineBuilders,
        Dictionary<string, object> outParams) {
        UnloadContents();
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.postProcessReferences = postProcessReferences;
        this.timelineBuilders = timelineBuilders;
        this.outParams = outParams;
        HasData = true;
    }

    internal void ClearData() {
        UnloadContents();
        assetBundleReferences = null;
        assetReferences = null;
        instanceReferences = null;
        postProcessReferences = null;
        timelineBuilders = null;
        outParams = null;
        HasData = false;
    }

    internal void LoadContents(ITimeConversion conversion, ILogger logger) {
        UnloadContents();
        
        if (!HasData)
            return;

        bool success = true;
        var watch = Stopwatch.StartNew();
        
        foreach (var reference in assetBundleReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in assetReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in instanceReferences)
            success = reference.TryLoad() && success;

        foreach (var reference in postProcessReferences)
            success = reference.TryLoad() && success;

        if (!success) {
            UnloadContents();
            
            return;
        }

        timelines = new Timeline[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateTimeline(conversion, out var curve)) {
                timelines[i] = curve;
                
                continue;
            }
            
            logger.LogWarning($"Failed to load {name}: Could not create timeline {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            UnloadContents();

            return;
        }
        
        if (active)
            Play();
        else
            Stop();

        loaded = true;
        watch.Stop();
        logger.LogMessage($"Successfully loaded {name} in {watch.ElapsedMilliseconds}ms");
    }

    internal void UnloadContents() {
        loaded = false;

        if (HasData) {
            foreach (var reference in postProcessReferences)
                reference.Unload();

            foreach (var reference in instanceReferences)
                reference.Unload();

            foreach (var reference in assetReferences)
                reference.Unload();

            foreach (var reference in assetBundleReferences)
                reference.Unload();
        }

        timelines = null;
    }
}
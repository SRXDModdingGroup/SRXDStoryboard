using System.Collections.Generic;
using System.Diagnostics;

namespace StoryboardSystem; 

public class Storyboard {
    public bool HasData { get; private set; }

    private bool active;
    private bool opened;
    private bool shouldOpenOnRecompile;
    private float lastTime;
    private string name;
    private string directory;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private LoadedExternalObjectReference[] externalObjectReferences;
    private List<TimelineBuilder> timelineBuilders;
    private Dictionary<string, object> outParams;
    private Binding[] bindings;

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

        if (opened) {
            foreach (var reference in postProcessReferences)
                reference.SetStoryboardActive(true);
        }
        
        Evaluate(lastTime, false);
    }

    internal void Stop() {
        active = false;

        if (!opened)
            return;
            
        foreach (var reference in postProcessReferences)
            reference.SetStoryboardActive(false);
    }

    internal void Evaluate(float time, bool triggerEvents) {
        lastTime = time;
        
        if (!opened || !active)
            return;

        foreach (var binding in bindings) {
            if (triggerEvents || !binding.IsEvent)
                binding.Evaluate(time);
        }
    }

    internal void Compile(bool force, ILogger logger) {
        if (HasData && !force)
            return;
        
        ClearData();
        
        if (Compiler.TryCompileFile(name, directory, logger, out var result))
            SetData(result);
    }

    internal void Recompile(bool force, IAssetBundleManager assetBundleManager, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        if (HasData && !force)
            return;
        
        Compile(force, logger);

        if (shouldOpenOnRecompile)
            Open(assetBundleManager, sceneManager, storyboardParams, logger);
    }

    internal void Open(IAssetBundleManager assetBundleManager, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        Close();
        shouldOpenOnRecompile = true;
        
        if (!HasData)
            return;

        bool success = true;
        var watch = Stopwatch.StartNew();
        
        foreach (var reference in assetBundleReferences)
            success = reference.TryLoad(assetBundleManager, logger) && success;
        
        foreach (var reference in assetReferences)
            success = reference.TryLoad(logger) && success;
        
        foreach (var reference in instanceReferences)
            success = reference.TryLoad(sceneManager, logger) && success;

        foreach (var reference in postProcessReferences)
            success = reference.TryLoad(sceneManager, logger) && success;

        foreach (var reference in externalObjectReferences)
            success = reference.TryLoad(storyboardParams, logger) && success;

        if (!success) {
            Close();
            
            return;
        }

        bindings = new Binding[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateBinding(storyboardParams, logger, out var binding)) {
                bindings[i] = binding;
                
                continue;
            }
            
            logger.LogWarning($"Failed to open {name}: Could not create timeline for {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            Close();

            return;
        }
        
        if (active)
            Play();
        else
            Stop();

        opened = true;
        watch.Stop();
        logger.LogMessage($"Successfully opened {name} in {watch.ElapsedMilliseconds}ms");
    }

    internal void Close(bool clearOpenOnRecompile = false) {
        opened = false;
        bindings = null;

        if (clearOpenOnRecompile)
            shouldOpenOnRecompile = false;

        if (!HasData)
            return;
        
        foreach (var reference in externalObjectReferences)
            reference.Unload();

        foreach (var reference in postProcessReferences)
            reference.Unload();

        foreach (var reference in instanceReferences)
            reference.Unload();

        foreach (var reference in assetReferences)
            reference.Unload();

        foreach (var reference in assetBundleReferences)
            reference.Unload();
    }

    private void SetData(StoryboardData data) {
        Close();
        assetBundleReferences = data.AssetBundleReferences;
        assetReferences = data.AssetReferences;
        instanceReferences = data.InstanceReferences;
        postProcessReferences = data.PostProcessReferences;
        externalObjectReferences = data.ExternalObjectReferences;
        timelineBuilders = data.TimelineBuilders;
        outParams = data.OutParams;
        HasData = true;
    }

    private void ClearData() {
        Close();
        assetBundleReferences = null;
        assetReferences = null;
        instanceReferences = null;
        postProcessReferences = null;
        externalObjectReferences = null;
        timelineBuilders = null;
        outParams = null;
        HasData = false;
    }
}
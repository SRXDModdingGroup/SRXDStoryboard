using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StoryboardSystem; 

public class Storyboard {
    public bool HasData { get; private set; }

    private bool active;
    private bool opened;
    private bool shouldOpenOnRecompile;
    private float lastTime;
    private string name;
    private string directory;
    private List<LoadedObjectReference> objectReferences;
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

    internal void Play(ISceneManager sceneManager) {
        active = true;
        sceneManager.Start(this);
        Evaluate(lastTime, false);
    }

    internal void Stop(ISceneManager sceneManager) {
        active = false;
        sceneManager.Stop(this);
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

    internal void Recompile(bool force, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        if (TryCompile(sceneManager, logger, force) && shouldOpenOnRecompile)
            Open(sceneManager, storyboardParams, logger);
    }

    internal void Open(ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        Close(sceneManager);
        shouldOpenOnRecompile = true;
        
        if (!HasData)
            return;
        
        logger.LogMessage($"Attempting to open {name}");

        bool success = true;
        var watch = Stopwatch.StartNew();

        foreach (var reference in objectReferences)
            success = reference.TryLoad(objectReferences, sceneManager, storyboardParams) && success;

        if (!success) {
            Close(sceneManager);
            
            return;
        }

        bindings = new Binding[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateBinding(objectReferences, storyboardParams, out var binding)) {
                bindings[i] = binding;
                
                continue;
            }
            
            logger.LogWarning($"Failed to open {name}: Could not create timeline for {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            Close(sceneManager);

            return;
        }
        
        if (active)
            Play(sceneManager);
        else
            Stop(sceneManager);

        opened = true;
        watch.Stop();
        logger.LogMessage($"Successfully opened {name} in {watch.ElapsedMilliseconds}ms");
    }

    internal void Close(ISceneManager sceneManager, bool clearOpenOnRecompile = false) {
        opened = false;
        bindings = null;

        if (clearOpenOnRecompile)
            shouldOpenOnRecompile = false;

        if (!HasData)
            return;

        for (int i = objectReferences.Count - 1; i >= 0; i--)
            objectReferences[i].Unload(sceneManager);
    }

    private void SetData(StoryboardData data, ISceneManager sceneManager) {
        ClearData(sceneManager);
        objectReferences = data.ObjectReferences;
        timelineBuilders = data.TimelineBuilders;
        outParams = data.OutParams;
        HasData = true;
    }

    private void ClearData(ISceneManager sceneManager) {
        Close(sceneManager);
        objectReferences = null;
        timelineBuilders = null;
        outParams = null;
        HasData = false;
    }

    internal bool TryCompile(ISceneManager sceneManager, ILogger logger, bool force = false) {
        if (HasData && !force || !Compiler.TryCompileFile(name, directory, out var data))
            return false;
        
        SetData(data, sceneManager);
        logger.LogMessage($"Attempting to save {name}");
        
        string tempName = Path.GetTempFileName();
        bool success;
        var watch = Stopwatch.StartNew();

        try {
            using var writer = new BinaryWriter(File.Create(tempName));
            
            success = data.TrySerialize(writer);
        }
        catch (IOException e) {
            logger.LogError(e.Message);
            success = false;
        }
        
        watch.Stop();

        if (success) {
            File.Copy(tempName, Path.Combine(directory, Path.ChangeExtension(name, ".bin")), true);
            logger.LogMessage($"Successfully saved {name} in {watch.ElapsedMilliseconds}ms");
        }
        else
            logger.LogWarning($"Failed to save {name}");

        return true;
    }

    internal bool TryLoad(ISceneManager sceneManager, ILogger logger, bool force = false) {
        if (HasData && !force)
            return true;
        
        string path = Path.Combine(directory, Path.ChangeExtension(name, ".bin"));

        if (!File.Exists(path))
            return false;
        
        logger.LogMessage($"Attempting to load {name}");

        StoryboardData data;
        bool success;
        var watch = Stopwatch.StartNew();

        try {
            using var reader = new BinaryReader(File.OpenRead(path));
            
            success = StoryboardData.TryDeserialize(reader, out data);
        }
        catch (IOException e) {
            logger.LogError(e.Message);
            success = false;
            data = null;
        }
        
        watch.Stop();

        if (success) {
            logger.LogMessage($"Successfully loaded {name} in {watch.ElapsedMilliseconds}ms");
            SetData(data, sceneManager);
        }
        else
            logger.LogWarning($"Failed to load {name}");

        return success;
    }
}
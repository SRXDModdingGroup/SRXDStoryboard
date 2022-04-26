using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StoryboardSystem; 

public class Storyboard {
    private bool hasData;
    private bool active;
    private bool opened;
    private bool shouldOpenOnRecompile;
    private float lastTime;
    private string name;
    private string directory;
    private List<LoadedObjectReference> objectReferences;
    private Dictionary<Identifier, List<Identifier>> bindingIdentifiers;
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
        
        if (!hasData)
            return;
        
        logger.LogMessage($"Attempting to open {name}");

        bool success = true;
        var watch = Stopwatch.StartNew();

        foreach (var reference in objectReferences)
            success = reference.TryLoad(objectReferences, bindingIdentifiers, storyboardParams) && success;

        if (!success) {
            Close(sceneManager);
            
            return;
        }

        bindings = new Binding[bindingIdentifiers.Count];

        int i = 0;
        
        foreach (var pair in bindingIdentifiers) {
            success = Binder.TryCreateBinding(pair, objectReferences, out bindings[i]) && success;
            i++;
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
        
        if (clearOpenOnRecompile)
            shouldOpenOnRecompile = false;

        if (bindings != null) {
            foreach (var binding in bindings)
                binding.ResetProperties();
        }
        
        bindings = null;

        if (!hasData)
            return;

        for (int i = objectReferences.Count - 1; i >= 0; i--)
            objectReferences[i].Unload(sceneManager);
    }

    private void SetData(StoryboardData data, ISceneManager sceneManager) {
        ClearData(sceneManager);
        objectReferences = data.ObjectReferences;
        bindingIdentifiers = data.BindingIdentifiers;
        outParams = data.OutParams;
        hasData = true;
    }

    private void ClearData(ISceneManager sceneManager) {
        Close(sceneManager);
        objectReferences = null;
        bindingIdentifiers = null;
        outParams = null;
        hasData = false;
    }

    internal bool TryCompile(ISceneManager sceneManager, ILogger logger, bool force = false) {
        if (hasData && !force || !Compiler.TryCompileFile(name, directory, out var data))
            return false;
        
        SetData(data, sceneManager);
        logger.LogMessage($"Attempting to save {name}");

        bool success;
        var watch = Stopwatch.StartNew();

        try {
            var stream = new MemoryStream(1 << 16);

            success = data.TrySerialize(new BinaryWriter(stream));

            if (success) {
                using var encoder = new LZSSEncoder(File.Create(Path.Combine(directory, Path.ChangeExtension(name, ".bin"))));

                stream.WriteTo(encoder);
            }
        }
        catch (IOException e) {
            logger.LogError(e.Message);
            success = false;
        }
        
        watch.Stop();
        
        if (success)
            logger.LogMessage($"Successfully saved {name} in {watch.ElapsedMilliseconds}ms");
        else
            logger.LogWarning($"Failed to save {name}");

        return true;
    }

    internal bool TryLoad(ISceneManager sceneManager, ILogger logger, bool force = false) {
        if (hasData && !force)
            return true;
        
        string path = Path.Combine(directory, Path.ChangeExtension(name, ".bin"));

        if (!File.Exists(path))
            return false;
        
        logger.LogMessage($"Attempting to load {name}");

        StoryboardData data;
        bool success;
        var watch = Stopwatch.StartNew();
        
        try {
            using var reader = new BinaryReader(new LZSSDecoder(File.OpenRead(path)));
            
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
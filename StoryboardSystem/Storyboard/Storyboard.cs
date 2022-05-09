using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StoryboardSystem; 

public class Storyboard {
    private bool active;
    private bool opened;
    private float lastTime;
    private string name;
    private string directory;
    private string txtPath;
    private string binPath;
    private StoryboardData data;
    private Binding[] bindings;
    private ISceneManager sceneManager;

    internal Storyboard(
        string name,
        string directory) {
        this.name = name;
        this.directory = directory;
        txtPath = Path.Combine(directory, Path.ChangeExtension(name, ".txt"));
        binPath = Path.Combine(directory, Path.ChangeExtension(name, ".bin"));
    }
    
    public bool TryGetOutParam<T>(string name, out T value) {
        if (data == null && !TryLoad() || !data.OutParams.TryGetValue(name, out object obj) || obj is not T cast) {
            value = default;

            return false;
        }

        value = cast;

        return true;
    }

    public void Play() {
        active = true;
        
        if (!opened)
            return;
        
        sceneManager.Start(this);
        Evaluate(lastTime, false);
    }

    public void Stop() {
        active = false;
        
        if (!opened)
            return;
        
        sceneManager.Stop(this);
    }

    public void Evaluate(float time, bool triggerEvents) {
        lastTime = time;
        
        if (!opened || !active)
            return;

        foreach (var binding in bindings) {
            if (triggerEvents || !binding.IsEvent)
                binding.Evaluate(time);
        }
    }

    public void Open(ISceneManager sceneManager) {
        Close();
        
        if (data == null && !TryLoad())
            return;

        this.sceneManager = sceneManager;
        StoryboardManager.Instance.Logger.LogMessage($"Attempting to open {name}");

        bool success = true;
        var watch = Stopwatch.StartNew();

        foreach (var reference in data.ObjectReferences)
            success = reference.TryLoad(data.ObjectReferences, data.BindingIdentifiers, sceneManager) && success;

        if (!success) {
            Close();
            
            return;
        }

        bindings = new Binding[data.BindingIdentifiers.Count];

        int i = 0;
        
        foreach (var pair in data.BindingIdentifiers) {
            success = Binder.TryCreateBinding(pair, data.ObjectReferences, out bindings[i]) && success;
            i++;
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
        StoryboardManager.Instance.Logger.LogMessage($"Successfully opened {name} in {watch.ElapsedMilliseconds}ms");
    }

    public void Close() {
        if (!opened)
            return;
        
        opened = false;

        if (bindings != null) {
            foreach (var binding in bindings)
                binding.ResetProperties();
        }
        
        bindings = null;

        if (data == null) {
            sceneManager = null;
            
            return;
        }

        for (int i = data.ObjectReferences.Count - 1; i >= 0; i--)
            data.ObjectReferences[i].Unload(sceneManager);

        sceneManager = null;
    }

    public void Recompile() => TryCompile(true);

    private void SetData(StoryboardData data) {
        ClearData();
        this.data = data;
    }

    private void ClearData() {
        Close();
        data = null;
    }

    private bool TryLoad(bool force = false) {
        if (data != null && !force)
            return true;

        if (!File.Exists(binPath) && !TryCompile(force))
            return false;
        
        StoryboardManager.Instance.Logger.LogMessage($"Attempting to load {name}");

        StoryboardData newData;
        bool success;
        var watch = Stopwatch.StartNew();
        
        try {
            using var reader = new BinaryReader(new LZSSDecoder(File.OpenRead(binPath)));
            
            success = StoryboardData.TryDeserialize(reader, out newData);
        }
        catch (IOException e) {
            StoryboardManager.Instance.Logger.LogError(e.Message);
            success = false;
            newData = null;
        }
        
        watch.Stop();
        
        if (success) {
            StoryboardManager.Instance.Logger.LogMessage($"Successfully loaded {name} in {watch.ElapsedMilliseconds}ms");
            SetData(newData);
        }
        else
            StoryboardManager.Instance.Logger.LogWarning($"Failed to load {name}");

        return success;
    }

    private bool TryCompile(bool force = false) {
        if (data != null && !force || !Compiler.TryCompileFile(name, directory, out var newData))
            return false;
        
        SetData(newData);
        StoryboardManager.Instance.Logger.LogMessage($"Attempting to save {name}");

        bool success;
        var watch = Stopwatch.StartNew();
        string tempName = Path.GetTempFileName();

        try {
            using var writer = new BinaryWriter(new LZSSEncoder(File.Create(tempName)));

            success = data.TrySerialize(writer);
            writer.Close();

            if (success)
                File.Copy(tempName, binPath);
        }
        catch (IOException e) {
            StoryboardManager.Instance.Logger.LogError(e.Message);
            success = false;
        }
        finally {
            File.Delete(tempName);
        }
        
        watch.Stop();
        
        if (success)
            StoryboardManager.Instance.Logger.LogMessage($"Successfully saved {name} in {watch.ElapsedMilliseconds}ms");
        else
            StoryboardManager.Instance.Logger.LogWarning($"Failed to save {name}");

        return true;
    }
}
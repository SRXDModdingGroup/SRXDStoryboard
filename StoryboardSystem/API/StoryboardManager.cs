using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

public sealed class StoryboardManager {
    private static StoryboardManager instance;
    public static StoryboardManager Instance {
        get {
            instance ??= new StoryboardManager();

            return instance;
        }
    }

    internal ILogger Logger { get; private set; } = new DefaultLogger();
    
    internal ISceneManager SceneManager { get; private set; }

    internal Dictionary<string, IStoryboardExtension> Extensions { get; } = new();

    private Storyboard currentStoryboard;
    private Dictionary<string, Storyboard> storyboards = new();
    

    public void Initialize(ISceneManager sceneManager,  ILogger logger) {
        SceneManager = sceneManager;
        Logger = logger;
    }

    public void Play() => currentStoryboard?.Play(SceneManager);

    public void Stop() => currentStoryboard?.Stop(SceneManager);

    public void SetTime(float time, bool triggerEvents) {
        currentStoryboard?.Evaluate(time, triggerEvents);
        SceneManager.Update(time, triggerEvents);
    }

    public void SetCurrentStoryboard(Storyboard storyboard, IStoryboardParams storyboardParams) {
        if (currentStoryboard != null) {
            currentStoryboard.Stop(SceneManager);
            currentStoryboard.Close(SceneManager, true);
        }
        
        currentStoryboard = storyboard;
        
        if (storyboard == null)
            return;
        
        if (!storyboard.TryLoad(SceneManager, Logger))
            storyboard.TryCompile(SceneManager, Logger);
        
        storyboard.Open(SceneManager, storyboardParams, Logger);
    }

    public void RecompileCurrentStoryboard(IStoryboardParams storyboardParams)
        => currentStoryboard?.Recompile(true, SceneManager, storyboardParams, Logger);

    public bool TryGetStoryboard(string directory, string name, out Storyboard storyboard)
        => storyboards.TryGetValue(Path.Combine(directory, name), out storyboard);

    public bool TryGetCurrentStoryboard(out Storyboard storyboard) {
        storyboard = currentStoryboard;
        
        return storyboard != null;
    }

    public bool TryGetOrCreateStoryboard(string directory, string name, out Storyboard storyboard, bool forceCompile = false) {
        string key = Path.Combine(directory, name);
        
        if (storyboards.TryGetValue(key, out storyboard))
            return true;

        if (!File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".txt"))) && !File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".bin"))))
            return false;

        storyboard = new Storyboard(name, directory);
        storyboards.Add(key, storyboard);
        
        if (forceCompile || !storyboard.TryLoad(SceneManager, Logger))
            storyboard.TryCompile(SceneManager, Logger);

        return true;
    }

    public bool TryAddExtension(string key, IStoryboardExtension extension) {
        if (Extensions.ContainsKey(key))
            return false;
        
        Extensions.Add(key, extension);

        return true;
    }
}
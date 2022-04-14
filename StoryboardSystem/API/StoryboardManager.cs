using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

public class StoryboardManager : MonoBehaviour {
    public static StoryboardManager Instance { get; private set; }

    private ILogger logger;
    private IAssetBundleManager assetBundleManager;
    private ISceneManager sceneManager;
    private Storyboard currentStoryboard;
    private Dictionary<string, Storyboard> storyboards = new();

    public void Play() => currentStoryboard?.Play();

    public void Stop() => currentStoryboard?.Stop();

    public void SetTime(float time, bool triggerEvents) {
        currentStoryboard?.Evaluate(time, triggerEvents);
        sceneManager.Update(time, triggerEvents);
    }

    public void SetCurrentStoryboard(Storyboard storyboard, IStoryboardParams storyboardParams) {
        if (currentStoryboard != null) {
            currentStoryboard.Stop();
            currentStoryboard.Close(true);
        }
        
        currentStoryboard = storyboard;
        
        if (storyboard == null)
            return;
        
        storyboard.TryCompile(logger);
        storyboard.Open(assetBundleManager, sceneManager, storyboardParams, logger);
    }

    public void RecompileCurrentStoryboard(IStoryboardParams storyboardParams)
        => currentStoryboard?.Recompile(true, assetBundleManager, sceneManager, storyboardParams, logger);

    public bool TryGetStoryboard(string directory, string name, out Storyboard storyboard)
        => storyboards.TryGetValue(Path.Combine(directory, name), out storyboard);

    public bool TryGetCurrentStoryboard(out Storyboard storyboard) {
        storyboard = currentStoryboard;
        
        return storyboard != null;
    }

    public bool TryGetOrCreateStoryboard(string directory, string name, out Storyboard storyboard) {
        string key = Path.Combine(directory, name);
        
        if (storyboards.TryGetValue(key, out storyboard))
            return true;

        if (!File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".txt"))))
            return false;

        storyboard = new Storyboard(name, directory);
        storyboards.Add(key, storyboard);
        storyboard.TryCompile(logger);

        return true;
    }

    public static void Create(IAssetBundleManager assetBundleManager, ISceneManager sceneManager,  ILogger logger) {
        if (Instance != null)
            return;
        
        var gameObject = new GameObject("Storyboard Manager");
        
        Instance = gameObject.AddComponent<StoryboardManager>();
        Instance.assetBundleManager = assetBundleManager;
        Instance.sceneManager = sceneManager;
        Instance.logger = logger;
    }
}
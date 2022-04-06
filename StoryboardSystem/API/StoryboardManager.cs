using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

public class StoryboardManager : MonoBehaviour {
    public static StoryboardManager Instance { get; private set; }
    
    internal ILogger Logger { get; private set; }
    
    internal IAssetBundleManager AssetBundleManager { get; private set; }
    
    internal ISceneManager SceneManager { get; private set; }

    private Storyboard currentStoryboard;
    private Dictionary<string, Storyboard> storyboards = new();

    public void Play() {
        for (int i = 0; i < SceneManager.LayerCount; i++)
            SceneManager.GetLayerRoot(i).gameObject.SetActive(true);

        currentStoryboard?.Play();
    }

    public void Stop() {
        for (int i = 0; i < SceneManager.LayerCount; i++)
            SceneManager.GetLayerRoot(i).gameObject.SetActive(true);
        
        currentStoryboard?.Stop();
    }

    public void SetTime(float time, bool triggerEvents) {
        currentStoryboard?.Evaluate(time, triggerEvents);
        SceneManager.Update(time, triggerEvents);
    }

    public void SetCurrentStoryboard(Storyboard storyboard, ITimeConversion conversion) {
        if (currentStoryboard != null) {
            currentStoryboard.Stop();
            currentStoryboard.UnloadContents();
        }
        
        currentStoryboard = storyboard;
        
        if (storyboard == null)
            return;
        
        storyboard.Compile(false, Logger);
        storyboard.LoadContents(conversion, Logger);
    }

    public void RecompileCurrentStoryboard(ITimeConversion conversion)
        => currentStoryboard?.Recompile(true, conversion, Logger);

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
        storyboard.Compile(false, Logger);

        return true;
    }

    public static void Create(IAssetBundleManager assetBundleManager, ISceneManager sceneManager,  ILogger logger) {
        if (Instance != null)
            return;
        
        var gameObject = new GameObject("Storyboard Manager");
        
        Instance = gameObject.AddComponent<StoryboardManager>();
        Instance.AssetBundleManager = assetBundleManager;
        Instance.SceneManager = sceneManager;
        Instance.Logger = logger;
    }
}
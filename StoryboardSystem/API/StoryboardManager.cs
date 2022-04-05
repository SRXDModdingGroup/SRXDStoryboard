using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

public class StoryboardManager : MonoBehaviour {
    public static StoryboardManager Instance { get; private set; }
    
    internal Transform SceneRoot { get; private set; }
    
    internal ILogger Logger { get; private set; }
    
    internal IAssetBundleManager AssetBundleManager { get; private set; }
    
    internal IPostProcessingManager PostProcessingManager { get; private set; }

    private bool active;
    private float currentTime;
    private Storyboard loadedStoryboard;
    private Dictionary<string, Storyboard> storyboards = new();

    public void LoadStoryboard(string path, ITimeConversion timeConversion) {
        bool exists = storyboards.TryGetValue(path, out var storyboard);
        
        if (exists && storyboard == loadedStoryboard)
            return;
        
        UnloadStoryboard();

        if (!exists) {
            if (!Compiler.TryCompileFile(path, timeConversion, out storyboard))
                return;
            
            storyboards.Add(path, storyboard);
        }
        
        if (!storyboard.TryLoad(Logger)) {
            Logger.LogWarning($"Failed to load {path}");
            
            return;
        }
        
        Logger.LogMessage($"Successfully loaded {path}");
        loadedStoryboard = storyboard;
        SetTime(0f, false);
    }

    public void UnloadStoryboard() {
        if (loadedStoryboard == null)
            return;
        
        Stop();
        loadedStoryboard.Unload();
        loadedStoryboard = null;
    }

    public void Play() {
        if (loadedStoryboard == null)
            return;
        
        active = true;
        SceneRoot.gameObject.SetActive(true);
        loadedStoryboard.SetPostProcessingEnabled(true);
        SetTime(currentTime, false);
    }

    public void Stop() {
        if (loadedStoryboard == null)
            return;
        
        active = false;
        loadedStoryboard.SetPostProcessingEnabled(false);
        SceneRoot.gameObject.SetActive(false);
    }

    public void SetTime(float time, bool triggerEvents) {
        if (loadedStoryboard == null)
            return;

        currentTime = time;

        if (active)
            loadedStoryboard.Evaluate(time, triggerEvents);
    }

    public static void Create(Transform rootParent, ILogger logger, IAssetBundleManager assetBundleManager, IPostProcessingManager postProcessingManager) {
        if (Instance != null)
            return;
        
        var gameObject = new GameObject("Storyboard Manager");
        
        Instance = gameObject.AddComponent<StoryboardManager>();
        Instance.Logger = logger;
        Instance.AssetBundleManager = assetBundleManager;
        Instance.PostProcessingManager = postProcessingManager;
        Instance.SceneRoot = new GameObject("Scene Root").transform;
        Instance.SceneRoot.SetParent(rootParent, false);
        Instance.SceneRoot.gameObject.SetActive(false);
    }
}
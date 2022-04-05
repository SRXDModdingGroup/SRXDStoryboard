using System.Collections.Generic;
using System.IO;
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
    private Storyboard currentStoryboard;
    private Dictionary<string, Storyboard> storyboards = new();

    public void LoadStoryboard(string name, string directory, ITimeConversion timeConversion) {
        string key = Path.Combine(directory, name);
        
        if (!storyboards.TryGetValue(key, out var storyboard)) {
            storyboard = new Storyboard(name, directory);
            storyboards.Add(key, storyboard);
        }

        if (storyboard != currentStoryboard) {
            UnloadStoryboard();
            currentStoryboard = storyboard;
        }
        

        SetTime(0f, false);

        if (currentStoryboard.TryCompile(Logger, false))
            currentStoryboard.Load(timeConversion, Logger);
    }

    public void UnloadStoryboard() {
        Stop();
        currentStoryboard?.Unload();
    }

    public void RecompileStoryboard(ITimeConversion timeConversion) {
        if (currentStoryboard == null)
            return;
        
        if (currentStoryboard.TryCompile(Logger, true))
            currentStoryboard.Load(timeConversion, Logger);

        if (active)
            Play();
        else
            Stop();
    }

    public void Play() {
        active = true;
        SceneRoot.gameObject.SetActive(true);
        currentStoryboard?.SetEnabled(true);
        SetTime(currentTime, false);
    }

    public void Stop() {
        active = false;
        SceneRoot.gameObject.SetActive(false);
        currentStoryboard?.SetEnabled(false);
    }

    public void SetTime(float time, bool triggerEvents) {
        currentTime = time;

        if (active)
            currentStoryboard?.Evaluate(time, triggerEvents);
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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

public class StoryboardManager : MonoBehaviour {
    public static StoryboardManager Instance { get; private set; }
    
    internal IAssetBundleManager AssetBundleManager { get; private set; }
    
    internal IPostProcessingManager PostProcessingManager { get; private set; }

    private float lastTime;
    private Storyboard loadedStoryboard;
    private Dictionary<string, Storyboard> storyboards;
    private Action<string> errorCallback;

    private void Awake() {
        storyboards = new Dictionary<string, Storyboard>();
    }

    public void LoadStoryboard(string path, ITimeConversion timeConversion) {
        UnloadStoryboard();
        
        if (storyboards.TryGetValue(path, out var storyboard)) { }
        else if (Compiler.TryCompileFile(path, timeConversion, errorCallback, out storyboard))
            storyboards.Add(path, storyboard);
        else
            return;
        
        storyboard.Load(errorCallback);
        loadedStoryboard = storyboard;
        lastTime = 0f;
        loadedStoryboard.Evaluate(-1f, 0f, false);
    }

    public void UnloadStoryboard() {
        if (loadedStoryboard == null)
            return;
        
        loadedStoryboard.Unload();
        loadedStoryboard = null;
    }

    public void SetTime(float time, bool triggerEvents) {
        if (loadedStoryboard == null)
            return;
        
        loadedStoryboard.Evaluate(lastTime, time, triggerEvents);
        lastTime = time;
    }

    public static void Create(Transform root, IAssetBundleManager assetBundleManager, IPostProcessingManager postProcessingManager, Action<string> errorCallback) {
        if (Instance != null)
            return;
        
        var gameObject = new GameObject();
        
        gameObject.transform.SetParent(root, false);
        Instance = gameObject.AddComponent<StoryboardManager>();
        Instance.AssetBundleManager = assetBundleManager;
        Instance.PostProcessingManager = postProcessingManager;
        Instance.errorCallback = errorCallback;
    }
}
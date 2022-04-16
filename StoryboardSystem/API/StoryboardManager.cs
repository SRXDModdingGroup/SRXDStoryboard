﻿using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

public class StoryboardManager {
    public static StoryboardManager Instance { get; private set; }

    private ILogger logger;
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
            currentStoryboard.Close(sceneManager, true);
        }
        
        currentStoryboard = storyboard;
        
        if (storyboard == null)
            return;
        
        if (!storyboard.TryLoad(sceneManager, logger))
            storyboard.TryCompile(sceneManager, logger);
        
        storyboard.Open(sceneManager, storyboardParams, logger);
    }

    public void RecompileCurrentStoryboard(IStoryboardParams storyboardParams)
        => currentStoryboard?.Recompile(true, sceneManager, storyboardParams, logger);

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

        if (!File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".txt"))))
            return false;

        storyboard = new Storyboard(name, directory);
        storyboards.Add(key, storyboard);
        
        if (forceCompile || !storyboard.TryLoad(sceneManager, logger))
            storyboard.TryCompile(sceneManager, logger);

        return true;
    }

    public static void Create(ISceneManager sceneManager,  ILogger logger) {
        if (Instance != null)
            return;

        Instance = new StoryboardManager {
            sceneManager = sceneManager,
            logger = logger
        };
    }
}
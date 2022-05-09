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

    private Dictionary<string, Storyboard> storyboards = new();

    public void SetLogger(ILogger logger) => Logger = logger;

    public bool TryGetStoryboard(string directory, string name, out Storyboard storyboard)
        => storyboards.TryGetValue(Path.Combine(directory, name), out storyboard);

    public bool TryGetOrCreateStoryboard(string directory, string name, out Storyboard storyboard, bool forceCompile = false) {
        string key = Path.Combine(directory, name);
        
        if (storyboards.TryGetValue(key, out storyboard))
            return true;

        if (!File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".txt"))) && !File.Exists(Path.Combine(directory, Path.ChangeExtension(name, ".bin"))))
            return false;

        storyboard = new Storyboard(name, directory);
        storyboards.Add(key, storyboard);

        return true;
    }
}
using System.Collections.Generic;
using System.IO;
using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class Storyboard {
    private bool active;
    private bool opened;
    private float lastTime;
    private string name;
    private string directory;
    private string binPath;
    private StoryboardData data;
    private StoryboardScene scene;
    private List<EventController> eventBindings;
    private List<PropertyController> propertyBindings;

    public Storyboard(string name, string directory) {
        this.name = name;
        this.directory = directory;
        binPath = Path.Combine(directory, Path.ChangeExtension(name, ".bin"));
        eventBindings = new List<EventController>();
        propertyBindings = new List<PropertyController>();
    }

    public void Play() {
        active = true;
        
        if (!opened)
            return;
        
        Evaluate(lastTime, false);
    }

    public void Stop() => active = false;

    public void Evaluate(float time, bool triggerEvents) {
        lastTime = time;
        
        if (!opened || !active)
            return;

        foreach (var binding in propertyBindings)
            binding.Evaluate(time);

        foreach (var binding in eventBindings)
            binding.Evaluate(time, triggerEvents);
    }

    public void OpenScene(IAssetProvider assetProvider) {
        CloseScene();
        
        if (data == null || !assetProvider.TryGetAsset(data.AssetBundleName, data.ScenePrefabName, out var sceneAsset)
            || sceneAsset is not GameObject sceneObject || !sceneObject.HasComponent<StoryboardScene>())
            return;

        scene = ((GameObject) Object.Instantiate(sceneAsset)).GetComponent<StoryboardScene>();
        opened = true;
    }

    public void CloseScene() {
        if (!opened)
            return;
        
        opened = false;
        Object.Destroy(scene.gameObject);
        scene = null;
        eventBindings.Clear();
        propertyBindings.Clear();
    }
    
    private void SetData(StoryboardData data) {
        ClearData();
        this.data = data;
    }

    private void ClearData() {
        CloseScene();
        data = null;
    }
}
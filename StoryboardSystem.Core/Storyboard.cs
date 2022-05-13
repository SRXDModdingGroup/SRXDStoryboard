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
    private List<EventBinding> eventBindings;
    private List<PropertyBinding> propertyBindings;

    public Storyboard(string name, string directory) {
        this.name = name;
        this.directory = directory;
        binPath = Path.Combine(directory, Path.ChangeExtension(name, ".bin"));
        eventBindings = new List<EventBinding>();
        propertyBindings = new List<PropertyBinding>();
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

    public void Open(IAssetProvider assetProvider) {
        Close();
        
        if (data == null || !assetProvider.TryGetAsset(data.AssetBundleName, data.ScenePrefabName, out var sceneAsset)
            || sceneAsset is not GameObject sceneObject || !sceneObject.TryGetComponent(out scene))
            return;

        foreach ((string rigName, string propertyName, var eventCalls) in data.EventCalls) {
            if (scene.TryGetRig(rigName, out var rig) && rig.TryGetEventBinding(propertyName, out var actions))
                eventBindings.Add(new EventBinding(actions, eventCalls));
        }

        foreach ((string rigName, string propertyName, var curves) in data.Curves) {
            if (scene.TryGetRig(rigName, out var rig) && rig.TryGetValueBinding(propertyName, out var actions))
                propertyBindings.Add(new PropertyBinding(actions, curves));
        }

        opened = true;
    }

    public void Close() {
        if (!opened)
            return;
        
        opened = false;
        Object.Destroy(scene);
        scene = null;
        eventBindings.Clear();
        propertyBindings.Clear();
    }
    
    private void SetData(StoryboardData data) {
        ClearData();
        this.data = data;
    }

    private void ClearData() {
        Close();
        data = null;
    }
}
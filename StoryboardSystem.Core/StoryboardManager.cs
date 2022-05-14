using System.Collections.Generic;
using System.IO;
using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class StoryboardManager {
    private bool active;
    private bool opened;
    private float lastTime;
    private StoryboardScene scene;
    private List<EventController> eventControllers;
    private List<PropertyController> propertyControllers;

    public StoryboardManager() {
        eventControllers = new List<EventController>();
        propertyControllers = new List<PropertyController>();
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

        foreach (var binding in propertyControllers)
            binding.Evaluate(time);

        foreach (var binding in eventControllers)
            binding.Evaluate(time, triggerEvents);
    }

    public void OpenScene(GameObject prefab) {
        CloseScene();
        
        if (!prefab.HasComponent<StoryboardScene>())
            return;

        scene = Object.Instantiate(prefab).GetComponent<StoryboardScene>();
        opened = true;
    }

    public void CloseScene() {
        if (!opened)
            return;
        
        ClearData();
        Object.Destroy(scene.gameObject);
        scene = null;
        opened = false;
    }
    
    public void SetData(StoryboardData data) {
        ClearData();
        
        if (!opened)
            return;

        foreach ((string rigName, int rigIndex, string eventName, var eventCalls) in data.EventCalls) {
            if (scene.TryGetRig(rigName, rigIndex, out var rig) && rig.TryGetEventBinding(eventName, out var binding))
                eventControllers.Add(new EventController(binding, eventCalls));
        }
        
        foreach ((string rigName, int rigIndex, string propertyName, var curves) in data.Curves) {
            if (scene.TryGetRig(rigName, rigIndex, out var rig) && rig.TryGetPropertyBinding(propertyName, out var binding))
                propertyControllers.Add(new PropertyController(binding, curves));
        }
    }

    public void ClearData() {
        eventControllers.Clear();
        propertyControllers.Clear();
    }
}
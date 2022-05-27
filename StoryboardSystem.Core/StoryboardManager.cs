using System.Collections.Generic;
using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class StoryboardManager {
    private bool active;
    private bool opened;
    private float lastTime;
    private StoryboardScene scene;
    private List<EventController> eventControllers;
    private List<CurveController> propertyControllers;

    public StoryboardManager() {
        eventControllers = new List<EventController>();
        propertyControllers = new List<CurveController>();
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

        foreach (var controller in propertyControllers)
            controller.Evaluate(time);

        foreach (var controller in eventControllers)
            controller.Evaluate(time, triggerEvents);
    }

    public void OpenScene(GameObject prefab) {
        CloseScene();
        
        if (prefab.GetComponent<StoryboardScene>() == null)
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

        foreach (var reference in data.EventCalls) {
            if (scene.TryGetRig(reference.RigKey, reference.RigIndex, out var rig))
                eventControllers.Add(new EventController(rig, reference.Value));
        }
        
        foreach (var reference in data.Curves) {
            if (scene.TryGetRig(reference.RigKey, reference.RigIndex, out var rig))
                propertyControllers.Add(new CurveController(rig, reference.Value));
        }
    }

    public void ClearData() {
        eventControllers.Clear();
        propertyControllers.Clear();
    }
}
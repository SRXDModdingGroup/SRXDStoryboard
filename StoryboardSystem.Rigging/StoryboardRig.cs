using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Rigging;

public class StoryboardRig : MonoBehaviour {
    [SerializeField] private string name;
    [SerializeField] private RigProperty[] properties;
    [SerializeField] private RigEvent[] events;

    private Dictionary<string, List<Action<Vector4>>> valueBindings;
    private Dictionary<string, List<Action<List<Vector4>>>> eventBindings;

    public void BindProperty(string propertyName, Action<Vector4> action) {
        if (valueBindings.TryGetValue(propertyName, out var actions))
            actions.Add(action);
    }

    public void BindEvent(string propertyName, Action<List<Vector4>> action) {
        if (eventBindings.TryGetValue(propertyName, out var actions))
            actions.Add(action);
    }

    public bool TryGetValueBinding(string propertyName, out List<Action<Vector4>> actions) => valueBindings.TryGetValue(propertyName, out actions);

    public bool TryGetEventBinding(string propertyName, out List<Action<List<Vector4>>> actions) => eventBindings.TryGetValue(propertyName, out actions);

    private void Awake() {
        valueBindings = new Dictionary<string, List<Action<Vector4>>>();
        eventBindings = new Dictionary<string, List<Action<List<Vector4>>>>();

        foreach (var rigProperty in properties) {
            if (!valueBindings.ContainsKey(rigProperty.name))
                valueBindings.Add(rigProperty.name, new List<Action<Vector4>>());
        }

        foreach (var rigEvent in events) {
            if (!eventBindings.ContainsKey(rigEvent.name))
                eventBindings.Add(rigEvent.name, new List<Action<List<Vector4>>>());
        }
        
        foreach (var rigTarget in GetComponentsInChildren<RigTarget>())
            rigTarget.AssignRig(this);
    }
}
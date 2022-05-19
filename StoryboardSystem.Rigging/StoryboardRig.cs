using System;
using System.Collections.Generic;
using StoryboardSystem.Core;
using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class StoryboardRig {
    private Dictionary<string, EventBinding> eventBindings;
    private Dictionary<string, PropertyBinding> propertyBindings;

    public StoryboardRig(RigSettings settings) {
        eventBindings = new Dictionary<string, EventBinding>();
        propertyBindings = new Dictionary<string, PropertyBinding>();

        foreach (var eventSettings in settings.events)
            eventBindings.Add(eventSettings.key, new EventBinding());

        foreach (var propertySettings in settings.properties)
            propertyBindings.Add(propertySettings.key, new PropertyBinding());
    }

    public void BindEvent(string key, Action<List<Vector3>> action) {
        if (eventBindings.TryGetValue(key, out var binding))
            binding.Bind(action);
    }
    
    public void BindProperty(string key, Action<Vector3> action) {
        if (propertyBindings.TryGetValue(key, out var binding))
            binding.Bind(action);
    }

    public void ClearBindings() {
        eventBindings.Clear();
        propertyBindings.Clear();
    }

    public bool TryGetEventBinding(string key, out EventBinding binding) => eventBindings.TryGetValue(key, out binding);

    public bool TryGetPropertyBinding(string key, out PropertyBinding binding) => propertyBindings.TryGetValue(key, out binding);
}
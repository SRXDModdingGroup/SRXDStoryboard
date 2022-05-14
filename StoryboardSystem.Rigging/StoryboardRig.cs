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
            eventBindings.Add(eventSettings.name, new EventBinding());

        foreach (var propertySettings in settings.properties)
            propertyBindings.Add(propertySettings.name, new PropertyBinding());
    }

    public void BindEvent(string name, Action<List<Vector4>> action) {
        if (eventBindings.TryGetValue(name, out var binding))
            binding.Bind(action);
    }
    
    public void BindProperty(string name, Action<Vector4> action) {
        if (propertyBindings.TryGetValue(name, out var binding))
            binding.Bind(action);
    }

    public void ClearBindings() {
        eventBindings.Clear();
        propertyBindings.Clear();
    }

    public bool TryGetEventBinding(string name, out EventBinding binding) => eventBindings.TryGetValue(name, out binding);

    public bool TryGetPropertyBinding(string name, out PropertyBinding binding) => propertyBindings.TryGetValue(name, out binding);
}
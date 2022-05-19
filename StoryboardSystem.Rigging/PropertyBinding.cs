using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class PropertyBinding {
    private List<Action<Vector3>> actions = new();

    public void Bind(Action<Vector3> action) => actions.Add(action);

    public void Set(Vector3 value) {
        foreach (var action in actions)
            action(value);
    }
}
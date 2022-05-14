using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class PropertyBinding {
    private List<Action<Vector4>> actions = new();

    public void Bind(Action<Vector4> action) => actions.Add(action);

    public void Set(Vector4 value) {
        foreach (var action in actions)
            action(value);
    }
}
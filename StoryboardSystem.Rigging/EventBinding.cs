using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class EventBinding {
    private List<Action<List<Vector4>>> actions = new();

    public void Bind(Action<List<Vector4>> action) => actions.Add(action);

    public void Execute(List<Vector4> parameters) {
        foreach (var action in actions)
            action(parameters);
    }
}
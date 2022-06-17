using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualizerSystem.Rigging; 

public class VisualizerRig {
    private List<Action<List<Vector3>>> actions = new();

    public void Bind(Action<List<Vector3>> action) => actions.Add(action);

    public void ClearBindings() => actions.Clear();

    public void Execute(List<Vector3> parameters) {
        foreach (var action in actions)
            action(parameters);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class EventBinding {
    public List<Action<List<Vector4>>> Actions { get; }
    
    public List<EventCall> Calls { get; }

    private int lastCallIndex;

    public EventBinding(List<Action<List<Vector4>>> actions, List<EventCall> calls) {
        Actions = actions;
        Calls = calls;
    }

    public void Evaluate(double time, bool trigger) {
        while (true) {
            if (lastCallIndex < Calls.Count - 1 && time >= Calls[lastCallIndex + 1].Time) {
                lastCallIndex++;

                if (!trigger)
                    continue;
                
                foreach (var action in Actions)
                    action(Calls[lastCallIndex].Parameters);
            }
            else if (lastCallIndex >= 0 && time < Calls[lastCallIndex].Time)
                lastCallIndex--;
            else
                return;
        }
    }
}
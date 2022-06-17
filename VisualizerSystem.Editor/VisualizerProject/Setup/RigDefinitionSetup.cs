using System.Collections.Generic;
using VisualizerSystem.Rigging;

namespace VisualizerSystem.Editor; 

public class RigDefinitionSetup {
    public List<RigEventSetup> Events { get; }

    public RigDefinitionSetup(List<RigEventSetup> events) {
        Events = events;
    }

    public RigDefinitionSetup(RigDefinition definition) {
        Events = new List<RigEventSetup>();

        foreach (var @event in definition.events)
            Events.Add(new RigEventSetup(@event));
    }
}
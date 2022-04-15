﻿using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class TimelineBuilder {
    public string Name { get; }
    
    private List<Identifier> identifiers = new();
    private List<KeyframeBuilder> keyframeBuilders = new();
    
    public TimelineBuilder(string name) => Name = name;

    public void AddIdentifier(Identifier identifier) {
        if (!identifiers.Contains(identifier))
            identifiers.Add(identifier);
    }

    public void AddKey(Timestamp time, object value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public bool TryCreateBinding(List<LoadedObjectReference> objectReferences, IStoryboardParams sParams, ILogger logger, out Binding binding) {
        if (identifiers.Count == 0 || keyframeBuilders.Count == 0) {
            binding = null;
            logger.LogWarning($"Error creating binding for timeline {Name}: No identifiers or keyframes found");

            return false;
        }

        var properties = new Property[identifiers.Count];

        for (int i = 0; i < identifiers.Count; i++) {
            if (Binder.TryBindProperty(identifiers[i], objectReferences, logger, out properties[i]))
                continue;
            
            binding = null;
            logger.LogWarning($"Error creating binding for timeline {Name}: Could not bind property for {identifiers[i]}");

            return false;
        }

        return properties[0].TryCreateBinding(properties, keyframeBuilders, sParams, logger, out binding);
    }

    public bool TrySerialize(BinaryWriter writer, List<LoadedObjectReference> objectReferences) {
        writer.Write(Name);
        
        foreach (var identifier in identifiers)
            identifier.Serialize(writer);

        foreach (var keyframeBuilder in keyframeBuilders) {
            if (!keyframeBuilder.TrySerialize(writer))
                return false;
        }

        return true;
    }
}
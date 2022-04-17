using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class TimelineBuilder {
    public string Name { get; }
    
    private List<Identifier> identifiers;
    private List<KeyframeBuilder> keyframeBuilders;
    
    public TimelineBuilder(string name) {
        Name = name;
        identifiers = new List<Identifier>();
        keyframeBuilders = new List<KeyframeBuilder>();
    }

    public TimelineBuilder(string name, List<Identifier> identifiers, List<KeyframeBuilder> keyframeBuilders) {
        Name = name;
        this.identifiers = identifiers;
        this.keyframeBuilders = keyframeBuilders;
    }

    public void AddIdentifier(Identifier identifier) {
        if (!identifiers.Contains(identifier))
            identifiers.Add(identifier);
    }

    public void AddKey(Timestamp time, object value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public bool TrySerialize(BinaryWriter writer) {
        writer.Write(Name);
        writer.Write(identifiers.Count);
        
        foreach (var identifier in identifiers)
            identifier.Serialize(writer);
        
        writer.Write(keyframeBuilders.Count);

        foreach (var keyframeBuilder in keyframeBuilders) {
            if (!keyframeBuilder.TrySerialize(writer))
                return false;
        }

        return true;
    }

    public bool TryCreateBinding(List<LoadedObjectReference> objectReferences, IStoryboardParams sParams, out Binding binding) {
        if (identifiers.Count == 0 || keyframeBuilders.Count == 0) {
            binding = null;
            StoryboardManager.Instance.Logger.LogWarning($"Error creating binding for timeline {Name}: No identifiers or keyframes found");

            return false;
        }

        var properties = new Property[identifiers.Count];

        for (int i = 0; i < identifiers.Count; i++) {
            if (Binder.TryGetProperty(identifiers[i], objectReferences, out properties[i]))
                continue;
            
            binding = null;
            StoryboardManager.Instance.Logger.LogWarning($"Error creating binding for timeline {Name}: Could not bind property for {identifiers[i]}");

            return false;
        }

        return properties[0].TryCreateBinding(properties, keyframeBuilders, sParams, out binding);
    }

    public static bool TryDeserialize(BinaryReader reader, out TimelineBuilder timelineBuilder) {
        string name = reader.ReadString();
        int identifiersCount = reader.ReadInt32();
        var identifiers = new List<Identifier>(identifiersCount);

        for (int i = 0; i < identifiersCount; i++)
            identifiers.Add(Identifier.Deserialize(reader));

        int keyframeBuildersCount = reader.ReadInt32();
        var keyframeBuilders = new List<KeyframeBuilder>(keyframeBuildersCount);

        for (int i = 0; i < keyframeBuildersCount; i++) {
            if (!KeyframeBuilder.TryDeserialize(reader, out var keyframeBuilder)) {
                timelineBuilder = null;

                return false;
            }
            
            keyframeBuilders.Add(keyframeBuilder);
        }

        timelineBuilder = new TimelineBuilder(name, identifiers, keyframeBuilders);

        return true;
    }
}
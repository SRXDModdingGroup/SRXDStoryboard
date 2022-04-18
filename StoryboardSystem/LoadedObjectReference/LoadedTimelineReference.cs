using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class LoadedTimelineReference : LoadedObjectReference {
    public override object LoadedObject => controller;

    private Identifier identifier;
    private TimelineBuilder builder;
    private Controller controller;

    public LoadedTimelineReference(Identifier identifier, TimelineBuilder builder) {
        this.identifier = identifier;
        this.builder = builder;
    }

    public override void Unload(ISceneManager sceneManager) => controller = null;

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, Dictionary<Identifier, List<Identifier>> bindings, ISceneManager sceneManager, IStoryboardParams storyboardParams) {
        if (bindings.TryGetValue(identifier, out var properties)
            && Binder.TryResolveIdentifier(properties[0], objectReferences, out object obj)
            && obj is Property property
            && builder.TryCreateController(property, storyboardParams, out controller))
            return true;
        
        StoryboardManager.Instance.Logger.LogWarning($"Could not create timeline for {builder.Name}");

        return false;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.Timeline);
        identifier.Serialize(writer);
        
        return builder.TrySerialize(writer);
    }

    public new static bool TryDeserialize(BinaryReader reader, out LoadedObjectReference reference) {
        var identifier = Identifier.Deserialize(reader);
        
        if (TimelineBuilder.TryDeserialize(reader, out var builder)) {
            reference = new LoadedTimelineReference(identifier, builder);

            return true;
        }

        reference = null;

        return false;
    }
}
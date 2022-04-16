using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class StoryboardData {
    public List<LoadedObjectReference> ObjectReferences { get; }
    public List<TimelineBuilder> TimelineBuilders { get; }
    public Dictionary<string, object> OutParams { get; }

    public StoryboardData(
        List<LoadedObjectReference> objectReferences,
        List<TimelineBuilder> timelineBuilders,
        Dictionary<string, object> outParams) {
        ObjectReferences = objectReferences;
        TimelineBuilders = timelineBuilders;
        OutParams = outParams;
    }

    public bool TrySerialize(BinaryWriter writer) {
        writer.Write(ObjectReferences.Count);

        foreach (var reference in ObjectReferences)
            reference.Serialize(writer);

        writer.Write(TimelineBuilders.Count);
        
        foreach (var timelineBuilder in TimelineBuilders) {
            if (!timelineBuilder.TrySerialize(writer))
                return false;
        }

        writer.Write(OutParams.Count);

        foreach (var pair in OutParams) {
            writer.Write(pair.Key);

            if (!SerializationUtility.TrySerialize(pair.Value, writer))
                return false;
        }

        return true;
    }

    public static bool TryDeserialize(BinaryReader reader, out StoryboardData data) {
        int objectReferenceCount = reader.ReadInt32();
        var objectReferences = new List<LoadedObjectReference>(objectReferenceCount);

        for (int i = 0; i < objectReferenceCount; i++) {
            if (!LoadedObjectReference.TryDeserialize(reader, out var reference)) {
                data = null;
                
                return false;
            }

            objectReferences.Add(reference);
        }

        int timelineBuilderCount = reader.ReadInt32();
        var timelineBuilders = new List<TimelineBuilder>();

        for (int i = 0; i < timelineBuilderCount; i++) {
            if (!TimelineBuilder.TryDeserialize(reader, out var timelineBuilder)) {
                data = null;

                return false;
            }
            
            timelineBuilders.Add(timelineBuilder);
        }

        int outParamsCount = reader.ReadInt32();
        var outParams = new Dictionary<string, object>();

        for (int i = 0; i < outParamsCount; i++) {
            string key = reader.ReadString();

            if (!SerializationUtility.TryDeserialize(reader, out object value)) {
                data = null;

                return false;
            }
            
            outParams.Add(key, value);
        }

        data = new StoryboardData(objectReferences, timelineBuilders, outParams);

        return true;
    }
}
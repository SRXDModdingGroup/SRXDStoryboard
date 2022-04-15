using System;
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
        
        foreach (var builder in TimelineBuilders) {
            if (!builder.TrySerialize(writer, ObjectReferences))
                return false;
        }

        return true;
    }
}
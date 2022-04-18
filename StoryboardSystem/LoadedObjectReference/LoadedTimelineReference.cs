using System;
using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class LoadedTimelineReference : LoadedObjectReference {
    public override object LoadedObject => controller;

    private TimelineBuilder builder;
    private TimelineController controller;

    public LoadedTimelineReference(TimelineBuilder builder) {
        this.builder = builder;
    }

    public override void Unload(ISceneManager sceneManager) {
        throw new NotImplementedException();
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams storyboardParams) {
        throw new NotImplementedException();
    }

    public override bool TrySerialize(BinaryWriter writer) {
        throw new NotImplementedException();
    }

    public new static bool TryDeserialize(BinaryReader reader, out LoadedObjectReference reference) {
        if (TimelineBuilder.TryDeserialize(reader, out var builder)) {
            reference = new LoadedTimelineReference(builder);

            return true;
        }

        reference = null;

        return false;
    }
}
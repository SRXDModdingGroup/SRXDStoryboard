using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class LoadedPostProcessingReference : LoadedObjectReference {
    public override object LoadedObject => instance;

    private Identifier template;
    private Identifier camera;
    private PostProcessingInstance instance;

    public LoadedPostProcessingReference(Identifier template, Identifier camera) {
        this.template = template;
        this.camera = camera;
    }

    public override void Serialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.PostProcessing);
        template.Serialize(writer);
        camera.Serialize(writer);
    }

    public override void Unload(ISceneManager sceneManager) {
        instance?.Remove();
        instance = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (!Binder.TryResolveIdentifier(template, objectReferences, logger, out object obj))
            return false;
        
        if (obj is not Material material) {
            logger.LogWarning($"{template} is not a material");

            return false;
        }

        if (!Binder.TryResolveIdentifier(camera, objectReferences, logger, out obj))
            return false;

        if (obj is not Camera uCamera) {
            logger.LogWarning($"{camera} is not a camera");

            return false;
        }

        instance = new PostProcessingInstance(sceneManager, material, uCamera);
        instance.Add();

        return true;
    }

    public static LoadedPostProcessingReference Deserialize(BinaryReader reader) {
        var template = Identifier.Deserialize(reader);
        var camera = Identifier.Deserialize(reader);

        return new LoadedPostProcessingReference(template, camera);
    }
}
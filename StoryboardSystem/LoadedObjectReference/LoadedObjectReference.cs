using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem;

internal abstract class LoadedObjectReference {
    private static int instanceCounter;
    
    public abstract object LoadedObject { get; }
    
    private readonly int instanceId;

    protected LoadedObjectReference() {
        instanceId = instanceCounter;

        unchecked {
            instanceCounter++;
        }
    }

    public abstract void Serialize(BinaryWriter writer);

    public abstract void Unload(ISceneManager sceneManager);

    public abstract bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger);

    public override int GetHashCode() => instanceId;
}
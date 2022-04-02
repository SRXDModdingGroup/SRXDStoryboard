namespace StoryboardSystem.Core;

internal abstract class LoadedObjectReference {
    private static int instanceCounter;

    private readonly int instanceId;

    protected LoadedObjectReference() {
        instanceId = instanceCounter;

        unchecked {
            instanceCounter++;
        }
    }
    
    public abstract object LoadedObject { get; }
    
    public abstract void Load();

    public abstract void Unload();

    public override int GetHashCode() => instanceId;
}
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

    public override int GetHashCode() => instanceId;
}
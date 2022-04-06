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
    
    public abstract void Unload();
    
    public abstract bool TryLoad();

    public override int GetHashCode() => instanceId;
}
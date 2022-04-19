namespace StoryboardSystem; 

public interface ICustomObject {
    object Self { get; }

    void Cleanup();

    bool TryGetSubObject(string name, out object subObject);
}
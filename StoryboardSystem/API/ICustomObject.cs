namespace StoryboardSystem; 

public interface ICustomObject {
    object Self { get; }

    bool TryGetSubObject(string name, out object subObject);
}
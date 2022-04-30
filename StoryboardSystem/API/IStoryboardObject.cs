namespace StoryboardSystem; 

public interface IStoryboardObject {
    void Cleanup();

    bool TryGetSubObject(string name, out object subObject);
}
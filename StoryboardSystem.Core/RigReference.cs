namespace StoryboardSystem.Core; 

public class RigReference<T> {
    public string RigKey { get; }
    
    public int RigIndex { get; }
    
    public string PropertyKey { get; }

    public T Value { get; }

    public RigReference(string rigKey, int rigIndex, string propertyKey, T value) {
        RigKey = rigKey;
        RigIndex = rigIndex;
        PropertyKey = propertyKey;
        Value = value;
    }
}
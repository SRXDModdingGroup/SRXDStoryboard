namespace StoryboardSystem; 

internal readonly struct CameraIdentifier {
    public Identifier Identifier { get; }
    
    public int Index { get; }

    public CameraIdentifier(Identifier identifier, int index) {
        Identifier = identifier;
        Index = index;
    }
}
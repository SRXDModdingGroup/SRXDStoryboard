namespace StoryboardSystem.Core; 

internal readonly struct Procedure {
    public int StartIndex { get; }
    
    public Name[] ArgNames { get; }

    public Procedure(int startIndex, Name[] argNames) {
        StartIndex = startIndex;
        ArgNames = argNames;
    }
}
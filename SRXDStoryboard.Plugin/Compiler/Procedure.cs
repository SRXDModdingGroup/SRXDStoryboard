namespace SRXDStoryboard.Plugin; 

public readonly struct Procedure {
    public int StartIndex { get; }
    
    public string[] ArgNames { get; }

    public Procedure(int startIndex, string[] argNames) {
        StartIndex = startIndex;
        ArgNames = argNames;
    }
}
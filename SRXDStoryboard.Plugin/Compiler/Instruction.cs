namespace SRXDStoryboard.Plugin; 

public readonly struct Instruction {
    public Timestamp Timestamp { get; }
    
    public Keyword Keyword { get; }
    
    public object[] Arguments { get; }

    public Instruction(Timestamp timestamp, Keyword keyword, object[] arguments) {
        Timestamp = timestamp;
        Keyword = keyword;
        Arguments = arguments;
    }
}
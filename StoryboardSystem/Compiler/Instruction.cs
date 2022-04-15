namespace StoryboardSystem; 

internal readonly struct Instruction {
    public Opcode Opcode { get; }
    
    public object[] Arguments { get; }
    
    public object[] ResolvedArguments { get; }
    
    public int LineIndex { get; }

    public Instruction(Opcode opcode, object[] arguments, object[] resolvedArguments, int lineIndex) {
        Opcode = opcode;
        Arguments = arguments;
        ResolvedArguments = resolvedArguments;
        LineIndex = lineIndex;
    }
}
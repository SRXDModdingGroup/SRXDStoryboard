namespace StoryboardSystem; 

internal readonly struct Instruction {
    public Opcode Opcode { get; }
    
    public Token[] Arguments { get; }
    
    public int LineIndex { get; }

    public Instruction(Opcode opcode, Token[] arguments, int lineIndex) {
        Opcode = opcode;
        Arguments = arguments;
        LineIndex = lineIndex;
    }
}
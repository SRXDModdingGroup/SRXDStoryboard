namespace StoryboardSystem; 

internal readonly struct Instruction {
    public Opcode Opcode { get; }
    
    public object[] Arguments { get; }
    
    public int LineIndex { get; }

    public Instruction(Opcode opcode, object[] arguments, int lineIndex) {
        Opcode = opcode;
        Arguments = arguments;
        LineIndex = lineIndex;
    }
}
namespace SRXDStoryboard.Plugin; 

public readonly struct Instruction {
    public Timestamp Timestamp { get; }
    
    public Opcode Opcode { get; }
    
    public object[] Arguments { get; }

    public Instruction(Timestamp timestamp, Opcode opcode, object[] arguments) {
        Timestamp = timestamp;
        Opcode = opcode;
        Arguments = arguments;
    }
}
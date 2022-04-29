namespace StoryboardSystem; 

public class OpcodeT : Token {
    public override TokenType Type => TokenType.Opcode;

    public Opcode Opcode { get; }
    
    public OpcodeT(Opcode opcode) {
        Opcode = opcode;
    }
}
namespace StoryboardSystem; 

internal class FuncCall : Token {
    public override TokenType Type => TokenType.FuncCall;

    public FuncName Name { get; }
    
    public Token[] Arguments { get; }

    public FuncCall(FuncName name, Token[] arguments) {
        Name = name;
        Arguments = arguments;
    }
}
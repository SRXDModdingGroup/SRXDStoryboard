namespace StoryboardSystem; 

public class Chain : Token {
    public override TokenType Type => TokenType.Chain;

    public int Length => chain.Length;
    
    private Token[] chain;

    public Chain(Token[] chain) => this.chain = chain;

    public Token this[int index] => chain[index];
}
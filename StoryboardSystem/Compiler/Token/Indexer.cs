namespace StoryboardSystem; 

internal class Indexer : Token {
    public override TokenType Type => TokenType.Indexer;

    public Token Token { get; }

    public Indexer(Token token) => Token = token;
}
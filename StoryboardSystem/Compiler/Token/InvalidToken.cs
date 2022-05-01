namespace StoryboardSystem; 

public class InvalidToken : Token {
    public override TokenType Type => TokenType.Invalid;

    public InvalidToken() => Success = false;
}
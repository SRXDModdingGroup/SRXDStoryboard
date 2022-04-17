namespace StoryboardSystem; 

internal class Constant : Token {
    public override TokenType Type => TokenType.Constant;

    public object Value { get; }
    
    public Constant(object value) => Value = value;
}
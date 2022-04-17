namespace StoryboardSystem; 

internal class ArrayT : Token {
    public override TokenType Type => TokenType.Array;
    
    public int Length => array.Length;
    
    private Token[] array;

    public ArrayT(Token[] array) => this.array = array;

    public Token this[int index] => array[index];
}
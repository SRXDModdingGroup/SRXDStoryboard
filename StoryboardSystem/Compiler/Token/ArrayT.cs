using System.Collections;
using System.Collections.Generic;

namespace StoryboardSystem; 

public class ArrayT : Token, IEnumerable<Token> {
    public override TokenType Type => TokenType.Array;
    
    public int Length => array.Length;
    
    private Token[] array;

    public ArrayT(Token[] array) => this.array = array;

    public Token this[int index] => array[index];

    public IEnumerator<Token> GetEnumerator() => ((IEnumerable<Token>) array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
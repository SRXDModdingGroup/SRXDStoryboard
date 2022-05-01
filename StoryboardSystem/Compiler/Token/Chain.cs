using System.Collections;
using System.Collections.Generic;

namespace StoryboardSystem; 

public class Chain : Token, IEnumerable<Token> {
    public override TokenType Type => TokenType.Chain;

    public int Length => chain.Length;
    
    private Token[] chain;

    public Chain(Token[] chain) => this.chain = chain;

    public Token this[int index] => chain[index];

    public IEnumerator<Token> GetEnumerator() => ((IEnumerable<Token>) chain).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
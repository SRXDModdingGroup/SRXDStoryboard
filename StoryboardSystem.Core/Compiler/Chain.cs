namespace StoryboardSystem.Core; 

internal readonly struct Chain {
    public int Length => chain.Length;
    
    private readonly object[] chain;

    public Chain(object[] chain) => this.chain = chain;

    public object this[int index] => chain[index];
}
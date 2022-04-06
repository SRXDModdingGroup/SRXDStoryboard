namespace StoryboardSystem; 

internal class Chain {
    public int Length => chain.Length;
    
    private readonly object[] chain;

    public Chain(object[] chain) => this.chain = chain;

    public object this[int index] => chain[index];
}
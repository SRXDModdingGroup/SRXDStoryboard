namespace StoryboardSystem.Core; 

public readonly struct NameChain {
    public int Length => chain.Length;
    
    private readonly string[] chain;

    public NameChain(string[] chain) => this.chain = chain;

    public string this[int index] => chain[index];
}
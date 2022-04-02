namespace StoryboardSystem.Core; 

internal readonly struct Binding {
    private const uint HASH_BIAS = 2166136261u;
    private const int HASH_COEFF = 486187739;
    
    private readonly LoadedObjectReference reference;
    private readonly BindingSequence sequence;
    private readonly int hash;

    public Binding(LoadedObjectReference reference, BindingSequence sequence) {
        this.reference = reference;
        this.sequence = sequence;
        hash = HashUtility.Combine(reference, sequence);
    }

    public override bool Equals(object obj) => obj is Binding other && this == other;

    public override int GetHashCode() => hash;

    public static bool operator ==(Binding a, Binding b) => a.reference == b.reference && a.sequence == b.sequence;

    public static bool operator !=(Binding a, Binding b) => !(a == b);
}
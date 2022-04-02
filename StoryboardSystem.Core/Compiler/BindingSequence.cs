namespace StoryboardSystem.Core; 

public readonly struct BindingSequence {
    public int Length => sequence.Length;
    
    private readonly object[] sequence;

    public BindingSequence(object[] sequence) => this.sequence = sequence;

    public object this[int index] => sequence[index];

    public override bool Equals(object obj) => obj is BindingSequence other && this == other;

    public override int GetHashCode() => HashUtility.Combine(sequence);

    public static bool operator ==(BindingSequence a, BindingSequence b) {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++) {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public static bool operator !=(BindingSequence a, BindingSequence b) => !(a == b);
}
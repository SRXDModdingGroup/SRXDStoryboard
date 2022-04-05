using System.Text;

namespace StoryboardSystem; 

internal readonly struct Identifier {
    public LoadedObjectReference Reference { get; }
    
    public object[] Sequence { get; }

    private readonly int hash;

    public Identifier(LoadedObjectReference reference, object[] sequence) {
        Reference = reference;
        Sequence = sequence;
        hash = HashUtility.Combine(reference, HashUtility.Combine(sequence));
    }

    public override bool Equals(object obj) => obj is Identifier other && this == other;

    public override int GetHashCode() => hash;

    public override string ToString() {
        var builder = new StringBuilder($"Binding_{Reference.GetHashCode()}");

        foreach (object item in Sequence) {
            switch (item) {
                case string str:
                    builder.Append($".{str}");
                    break;
                case int intVal:
                    builder.Append($"[{intVal}]");
                    break;
                case null:
                    builder.Append(".NULL");
                    break;
                default:
                    builder.Append($".{item}");
                    break;
            }
        }

        return builder.ToString();
    }

    public static bool operator ==(Identifier a, Identifier b) {
        if (a.hash != b.hash || a.Reference != b.Reference || a.Sequence.Length != b.Sequence.Length)
            return false;

        for (int i = 0; i < a.Sequence.Length; i++) {
            if (!a.Sequence[i].Equals(b.Sequence[i]))
                return false;
        }

        return true;
    }

    public static bool operator !=(Identifier a, Identifier b) => !(a == b);
}
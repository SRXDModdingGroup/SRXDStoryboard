using System.Text;

namespace StoryboardSystem; 

internal readonly struct Binding {
    public LoadedObjectReference Reference { get; }
    
    public object[] Sequence { get; }

    private readonly int hash;

    public Binding(LoadedObjectReference reference, object[] sequence) {
        Reference = reference;
        Sequence = sequence;
        hash = HashUtility.Combine(reference, HashUtility.Combine(sequence));
    }

    public override bool Equals(object obj) => obj is Binding other && this == other;

    public override int GetHashCode() => hash;

    public override string ToString() {
        var builder = new StringBuilder(Reference.LoadedObject.GetType().Name);

        foreach (object item in Sequence) {
            switch (item) {
                case string str:
                    builder.Append($".{str}");
                    break;
                case int intVal:
                    builder.Append($"[{intVal}]");
                    break;
                default:
                    builder.Append($".{item}");
                    break;
            }
        }

        return builder.ToString();
    }

    public static bool operator ==(Binding a, Binding b) {
        if (a.Reference != b.Reference || a.Sequence.Length != b.Sequence.Length)
            return false;

        for (int i = 0; i < a.Sequence.Length; i++) {
            if (a.Sequence[i] != b.Sequence[i])
                return false;
        }

        return true;
    }

    public static bool operator !=(Binding a, Binding b) => !(a == b);
}
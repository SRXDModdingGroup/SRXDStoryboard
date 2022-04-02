using System.Text;

namespace StoryboardSystem.Core; 

internal readonly struct Binding {
    private const uint HASH_BIAS = 2166136261u;
    private const int HASH_COEFF = 486187739;

    public LoadedObjectReference Reference { get; }
    
    public object[] Sequence { get; }

    private readonly int hash;

    public Binding(LoadedObjectReference reference, object[] sequence) {
        Reference = reference;
        Sequence = sequence;
        hash = HashUtility.Combine(reference, sequence);
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

    public static bool operator ==(Binding a, Binding b) => a.Reference == b.Reference && a.Sequence == b.Sequence;

    public static bool operator !=(Binding a, Binding b) => !(a == b);
}
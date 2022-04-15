using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StoryboardSystem; 

internal class Identifier {
    public int ReferenceIndex { get; }
    
    public object[] Sequence { get; }

    private readonly int hash;

    public Identifier(int referenceIndex, object[] sequence) {
        ReferenceIndex = referenceIndex;
        Sequence = sequence;
        hash = HashUtility.Combine(referenceIndex, HashUtility.Combine(sequence));
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(ReferenceIndex);
        writer.Write(Sequence.Length);
        
        foreach (object obj in Sequence) {
            switch (obj) {
                case int intVal:
                    writer.Write(false);
                    writer.Write(intVal);
                    continue;
                case string stringVal:
                    writer.Write(true);
                    writer.Write(stringVal);
                    continue;
            }
        }
    }

    public override bool Equals(object obj) => obj is Identifier other && this == other;

    public override int GetHashCode() => hash;

    public override string ToString() {
        var builder = new StringBuilder($"Reference_{ReferenceIndex}");

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
        if (a is null || b is null)
            return a is null && b is null;
        
        if (a.hash != b.hash || a.ReferenceIndex != b.ReferenceIndex || a.Sequence.Length != b.Sequence.Length)
            return false;

        for (int i = 0; i < a.Sequence.Length; i++) {
            if (!a.Sequence[i].Equals(b.Sequence[i]))
                return false;
        }

        return true;
    }

    public static bool operator !=(Identifier a, Identifier b) => !(a == b);

    public static Identifier Deserialize(BinaryReader reader) {
        int referenceIndex = reader.ReadInt32();
        int sequenceLength = reader.ReadInt32();
        object[] sequence = new object[sequenceLength];

        for (int i = 0; i < sequenceLength; i++) {
            if (reader.ReadBoolean())
                sequence[i] = reader.ReadString();
            else
                sequence[i] = reader.ReadInt32();
        }

        return new Identifier(referenceIndex, sequence);
    }
}
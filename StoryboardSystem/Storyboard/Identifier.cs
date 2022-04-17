using System.IO;
using System.Text;

namespace StoryboardSystem; 

internal class Identifier {
    public int ReferenceIndex { get; }
    
    public object[] Sequence { get; }

    private readonly int hash;
    private readonly string name;

    public Identifier(string name, int referenceIndex, object[] sequence) {
        this.name = name;
        ReferenceIndex = referenceIndex;
        Sequence = sequence;
        hash = HashUtility.Combine(referenceIndex, HashUtility.Combine(sequence));
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(name);
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

    public override string ToString() => name;

    public static bool operator ==(Identifier a, Identifier b) {
        if (a is null)
            return b is null;

        if (b is null)
            return false;
        
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
        string name = reader.ReadString();
        int referenceIndex = reader.ReadInt32();
        int sequenceLength = reader.ReadInt32();
        object[] sequence = new object[sequenceLength];

        for (int i = 0; i < sequenceLength; i++) {
            if (reader.ReadBoolean())
                sequence[i] = reader.ReadString();
            else
                sequence[i] = reader.ReadInt32();
        }

        return new Identifier(name, referenceIndex, sequence);
    }
}
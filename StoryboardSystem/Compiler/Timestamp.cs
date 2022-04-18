using System.IO;

namespace StoryboardSystem; 

internal readonly struct Timestamp {
    public static Timestamp Zero => new(Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero);
    
    public Fixed Measures { get; }

    public Fixed Beats { get; }
    
    public Fixed Ticks { get; }
    
    public Fixed Seconds { get; }

    public Timestamp(Fixed measures, Fixed beats, Fixed ticks, Fixed seconds) {
        Measures = measures;
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
    }

    public void Serialize(BinaryWriter writer, BinaryWriter buffer) {
        byte mask = 0;
        
        buffer.BaseStream.SetLength(0);
        Encode(Measures, 0);
        Encode(Beats, 2);
        Encode(Ticks, 4);
        Encode(Seconds, 6);
        
        writer.Write(mask);
        ((MemoryStream) buffer.BaseStream).WriteTo(writer.BaseStream);

        void Encode(Fixed value, int shift) {
            if (value == Fixed.Zero)
                return;

            if (Fixed.Floor(value) == value) {
                int intVal = Fixed.FloorToInt(value);

                if (intVal is >= sbyte.MinValue and <= sbyte.MaxValue) {
                    mask |= (byte) (1 << shift);
                    buffer.Write((sbyte) intVal);
                    
                    return;
                }

                if (intVal is >= short.MinValue and <= short.MaxValue) {
                    mask |= (byte) (2 << shift);
                    buffer.Write((short) intVal);

                    return;
                }
            }
            
            mask |= (byte) (3 << shift);
            buffer.Write(value);
        }
    }
    
    public override bool Equals(object obj) => obj is Timestamp other && this == other;

    public override int GetHashCode() => HashUtility.Combine(Measures, Beats, Ticks, Seconds);

    public static Timestamp Deserialize(BinaryReader reader) {
        byte mask = reader.ReadByte();
        var measures = Decode(0);
        var beats = Decode(2);
        var ticks = Decode(4);
        var seconds = Decode(6);

        return new Timestamp(measures, beats, ticks, seconds);

        Fixed Decode(int shift) => ((mask >> shift) & 3) switch {
            0 => Fixed.Zero,
            1 => (Fixed) (int) reader.ReadSByte(),
            2 => (Fixed) (int) reader.ReadInt16(),
            _ => reader.ReadFixed()
        };
    }

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Measures + b.Measures, a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds);

    public static Timestamp operator -(Timestamp a, Timestamp b) => new(a.Measures - b.Measures, a.Beats - b.Beats, a.Ticks - b.Ticks, a.Seconds - b.Seconds);

    public static bool operator ==(Timestamp a, Timestamp b) => a.Measures == b.Measures && a.Beats == b.Beats && a.Ticks == b.Ticks && a.Seconds == b.Seconds;

    public static bool operator !=(Timestamp a, Timestamp b) => a.Measures != b.Measures || a.Beats != b.Beats || a.Ticks != b.Ticks || a.Seconds != b.Seconds;

    public static Timestamp operator *(Timestamp t, int i) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(Timestamp t, float f) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    public static Timestamp operator *(float f, Timestamp t) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    
    public static Timestamp operator /(Timestamp t, int i) => new(t.Measures / i, t.Beats / i, t.Ticks / i, t.Seconds / i);
    public static Timestamp operator /(Timestamp t, float f) => new(t.Measures / f, t.Beats / f, t.Ticks / f, t.Seconds / f);
}
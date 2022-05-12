using System.IO;

namespace StoryboardSystem; 

internal readonly struct Timestamp {
    public static Timestamp Zero => new(Fixed.Zero, Fixed.Zero, Fixed.Zero);

    public Fixed Beats { get; }
    
    public Fixed Ticks { get; }
    
    public Fixed Seconds { get; }

    public Timestamp(Fixed beats, Fixed ticks, Fixed seconds) {
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
    }

    public void Serialize(BinaryWriter writer, BinaryWriter buffer) {
        byte mask = 0;
        
        buffer.BaseStream.SetLength(0);
        Encode(Beats, 0);
        Encode(Ticks, 2);
        Encode(Seconds, 4);
        
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

    public override int GetHashCode() => HashUtility.Combine(Beats, Ticks, Seconds);

    public static Timestamp Deserialize(BinaryReader reader) {
        byte mask = reader.ReadByte();
        var beats = Decode(0);
        var ticks = Decode(2);
        var seconds = Decode(4);

        return new Timestamp(beats, ticks, seconds);

        Fixed Decode(int shift) => ((mask >> shift) & 3) switch {
            0 => Fixed.Zero,
            1 => (Fixed) (int) reader.ReadSByte(),
            2 => (Fixed) (int) reader.ReadInt16(),
            _ => reader.ReadFixed()
        };
    }

    public static Timestamp Simplify(Timestamp timestamp) => new(timestamp.Beats + Fixed.Floor(timestamp.Ticks / 8), timestamp.Ticks % 8, timestamp.Seconds);

    public static Timestamp Min(Timestamp a, Timestamp b, int beatsPerMeasure, int ticksPerBeat) {
        var difference = Simplify(a - b);

        if (difference.Beats > Fixed.Zero)
            return a;
        
        if (difference.Beats < Fixed.Zero)
            return b;
        
        if (difference.Ticks > Fixed.Zero)
            return a;
        
        if (difference.Ticks < Fixed.Zero)
            return b;
        
        if (difference.Seconds > Fixed.Zero)
            return a;
        
        return b;
    }

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds);

    public static Timestamp operator -(Timestamp a, Timestamp b) => new(a.Beats - b.Beats, a.Ticks - b.Ticks, a.Seconds - b.Seconds);

    public static bool operator ==(Timestamp a, Timestamp b) => a.Beats == b.Beats && a.Ticks == b.Ticks && a.Seconds == b.Seconds;

    public static bool operator !=(Timestamp a, Timestamp b) => a.Beats != b.Beats || a.Ticks != b.Ticks || a.Seconds != b.Seconds;

    public static Timestamp operator *(Timestamp t, int i) => new(i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(Timestamp t, float f) => new(f * t.Beats, f * t.Ticks, f * t.Seconds);
    public static Timestamp operator *(float f, Timestamp t) => new(f * t.Beats, f * t.Ticks, f * t.Seconds);
    
    public static Timestamp operator /(Timestamp t, int i) => new(t.Beats / i, t.Ticks / i, t.Seconds / i);
    public static Timestamp operator /(Timestamp t, float f) => new(t.Beats / f, t.Ticks / f, t.Seconds / f);
}
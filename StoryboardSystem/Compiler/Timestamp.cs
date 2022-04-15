using System.IO;

namespace StoryboardSystem; 

internal class Timestamp {
    public static Timestamp Zero => new(0, 0, 0f, 0f);
    
    public float Measures { get; }

    public float Beats { get; }
    
    public float Ticks { get; }
    
    public float Seconds { get; }

    public Timestamp(float measures, float beats, float ticks, float seconds) {
        Measures = measures;
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
    }

    public void Serialize(BinaryWriter writer) {
        if (Measures == 0f)
            writer.Write(false);
        else {
            writer.Write(true);
            writer.Write(Measures);
        }
        
        if (Beats == 0f)
            writer.Write(false);
        else {
            writer.Write(true);
            writer.Write(Beats);
        }
        
        if (Ticks == 0f)
            writer.Write(false);
        else {
            writer.Write(true);
            writer.Write(Ticks);
        }
        
        if (Seconds == 0f)
            writer.Write(false);
        else {
            writer.Write(true);
            writer.Write(Seconds);
        }
    }

    public static Timestamp Deserialize(BinaryReader reader) {
        float measures;

        if (reader.ReadBoolean())
            measures = reader.ReadSingle();
        else
            measures = 0f;
        
        float beats;

        if (reader.ReadBoolean())
            beats = reader.ReadSingle();
        else
            beats = 0f;
        
        float ticks;

        if (reader.ReadBoolean())
            ticks = reader.ReadSingle();
        else
            ticks = 0f;
        
        float seconds;

        if (reader.ReadBoolean())
            seconds = reader.ReadSingle();
        else
            seconds = 0f;

        return new Timestamp(measures, beats, ticks, seconds);
    }

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Measures + b.Measures, a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds);

    public static Timestamp operator -(Timestamp a, Timestamp b) => new(a.Measures - b.Measures, a.Beats - b.Beats, a.Ticks - b.Ticks, a.Seconds - b.Seconds);

    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(Timestamp t, int i) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(float f, Timestamp t) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    public static Timestamp operator *(Timestamp t, float f) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    
    public static Timestamp operator /(Timestamp t, int i) => new(t.Measures / i, t.Beats / i, t.Ticks / i, t.Seconds / i);
    public static Timestamp operator /(Timestamp t, float f) => new(t.Measures / f, t.Beats / f, t.Ticks / f, t.Seconds / f);
}
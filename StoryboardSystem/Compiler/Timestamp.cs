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

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Measures + b.Measures, a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds);

    public static Timestamp operator -(Timestamp a, Timestamp b) => new(a.Measures - b.Measures, a.Beats - b.Beats, a.Ticks - b.Ticks, a.Seconds - b.Seconds);

    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(Timestamp t, int i) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds);
    public static Timestamp operator *(float f, Timestamp t) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    public static Timestamp operator *(Timestamp t, float f) => new(f * t.Measures, f * t.Beats, f * t.Ticks, f * t.Seconds);
    
    public static Timestamp operator /(Timestamp t, int i) => new(t.Measures / i, t.Beats / i, t.Ticks / i, t.Seconds / i);
    public static Timestamp operator /(Timestamp t, float f) => new(t.Measures / f, t.Beats / f, t.Ticks / f, t.Seconds / f);
}
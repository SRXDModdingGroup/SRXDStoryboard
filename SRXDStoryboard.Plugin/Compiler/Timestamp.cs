namespace SRXDStoryboard.Plugin; 

public readonly struct Timestamp {
    public static Timestamp Zero => new(0, 0f, 0f);

    public int Beats { get; }
    
    public float Ticks { get; }
    
    public float Seconds { get; }

    public Timestamp(int beats, float ticks, float seconds) {
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
    }

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds);

    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Beats, i * t.Ticks, i * t.Seconds);
}
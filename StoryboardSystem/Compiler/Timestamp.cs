namespace StoryboardSystem; 

internal class Timestamp {
    public static Timestamp Zero => new(0, 0, 0f, 0f);
    
    public int Measures { get; }

    public int Beats { get; }
    
    public float Ticks { get; }
    
    public float Seconds { get; }
    
    public int Order { get; }

    public Timestamp(int measures, int beats, float ticks, float seconds, int order = 0) {
        Measures = measures;
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
        Order = order;
    }

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Measures + b.Measures, a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds, a.Order);

    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Measures, i * t.Beats, i * t.Ticks, i * t.Seconds, t.Order);
}
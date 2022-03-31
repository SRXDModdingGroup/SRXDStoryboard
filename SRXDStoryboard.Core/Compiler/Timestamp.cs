namespace SRXDStoryboard.Core; 

public readonly struct Timestamp {
    public static Timestamp Zero => new(0, 0f, 0f);

    public int Beats { get; }
    
    public float Ticks { get; }
    
    public float Seconds { get; }
    
    public int Order { get; }

    public Timestamp(int beats, float ticks, float seconds, int order = 0) {
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
        Order = order;
    }

    public Timestamp WithOrder(int order) => new(Beats, Ticks, Seconds, order);

    public static Timestamp operator +(Timestamp a, Timestamp b) => new(a.Beats + b.Beats, a.Ticks + b.Ticks, a.Seconds + b.Seconds, a.Order);

    public static Timestamp operator *(int i, Timestamp t) => new(i * t.Beats, i * t.Ticks, i * t.Seconds, t.Order);
}
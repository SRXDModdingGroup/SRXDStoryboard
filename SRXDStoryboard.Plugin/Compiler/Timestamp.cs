using System.Text;

namespace SRXDStoryboard.Plugin; 

public readonly struct Timestamp {
    public float Beats { get; }
    
    public float Ticks { get; }
    
    public float Seconds { get; }

    public Timestamp(float beats, float ticks, float seconds) {
        Beats = beats;
        Ticks = ticks;
        Seconds = seconds;
    }
}
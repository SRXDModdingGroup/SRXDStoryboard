using StoryboardSystem;

namespace SRXDStoryboard; 

public class TimeConversion : ITimeConversion {
    private PlayableTrackData trackData;
    public TimeConversion(PlayableTrackData trackData) => this.trackData = trackData;

    public float Convert(int beats, float ticks, float seconds) => trackData.GetTimeForBeat(beats + 0.125f * ticks) + seconds;
}
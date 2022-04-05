using StoryboardSystem;

namespace SRXDStoryboard; 

public class TimeConversion : ITimeConversion {
    private PlayableTrackData trackData;
    public TimeConversion(PlayableTrackData trackData) => this.trackData = trackData;

    public float Convert(int measures, int beats, float ticks, float seconds) {
        var segments = trackData.TimeSignatureSegments;
        float beatsF = beats;
        
        if (segments.Length > 0) {
            int index = 0;

            while (index < segments.Length - 1 && segments[index + 1].startingBar <= measures)
                index++;
            
            beatsF += segments[index].startingBeat;
        }
        
        return trackData.GetTimeForBeat(beatsF + 0.125f * ticks) + seconds;
    }
}
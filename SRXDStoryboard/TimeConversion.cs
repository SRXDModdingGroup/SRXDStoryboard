using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class TimeConversion : ITimeConversion {
    private float[] beatArray;
    private TimeSignatureSegment[] segments;
    
    public TimeConversion(PlayableTrackData trackData) {
        beatArray = trackData.BeatArray;
        segments = trackData.TimeSignatureSegments;
    }

    public float Convert(int measures, int beats, float ticks, float seconds) {
        float beatsF = beats;
        
        if (segments.Length > 0) {
            int index0 = 0;

            while (index0 < segments.Length - 1 && segments[index0 + 1].startingBar <= measures)
                index0++;
            
            beatsF += segments[index0].startingBeat;
        }

        float beat = beatsF + 0.125f * ticks;
        int index1 = Mathf.Clamp(Mathf.FloorToInt(beat), 0, beatArray.Length - 2);
        
        return Mathf.LerpUnclamped(beatArray[index1], beatArray[index1 + 1], beat - index1) + seconds;
    }
}
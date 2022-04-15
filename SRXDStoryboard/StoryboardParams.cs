using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class StoryboardParams : IStoryboardParams {
    private float[] beatArray;
    private TimeSignatureSegment[] segments;
    
    public StoryboardParams(PlayableTrackData trackData) {
        beatArray = trackData.BeatArray;
        segments = trackData.TimeSignatureSegments;
    }

    public float Convert(float measures, float beats, float ticks, float seconds) {
        float beat = beats + 0.125f * ticks;
        
        if (segments.Length > 0) {
            int index0 = 0;

            while (index0 < segments.Length - 1 && segments[index0 + 1].startingBar <= measures)
                index0++;

            var segment = segments[index0];
            
            beat += segments[index0].startingBeat + (measures - segment.startingBar) * segment.ticksPerBar * segment.beatsPerTick;
        }
         
        int index1 = Mathf.Clamp(Mathf.FloorToInt(beat), 0, beatArray.Length - 2);
        float time = Mathf.LerpUnclamped(beatArray[index1], beatArray[index1 + 1], beat - index1) + seconds;
        
        return time;
    }

    public object GetExternalObject(string name) {
        switch (name) {
            case "CameraRoot":
                return MainCamera.Instance.transform;
        }

        return null;
    }
}
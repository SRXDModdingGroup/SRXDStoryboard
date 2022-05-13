using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class Curve {
    public double StartTime { get; }
    
    public double EndTime { get; }
    
    public List<Keyframe> Keyframes { get; }

    public Curve(double startTime, double endTime, List<Keyframe> keyframes) {
        StartTime = startTime;
        EndTime = endTime;
        Keyframes = keyframes;
    }

    public Vector4 Evaluate(double time, int index) {
        if (index < 0)
            index = 0;
        else if (index >= Keyframes.Count - 1)
            index = Keyframes.Count - 2;

        while (true) {
            var previous = Keyframes[index];

            if (time < previous.Time) {
                if (index == 0)
                    return previous.Value;
                
                index--;
                
                continue;
            }
            
            var next = Keyframes[index + 1];

            if (time >= next.Time) {
                if (index == Keyframes.Count - 2)
                    return next.Value;

                index++;
                
                continue;
            }

            var interpType = previous.InterpType;

            if (interpType == InterpType.Fixed)
                return previous.Value;

            float interp = (float) (time - previous.Time) / (float) (next.Time - previous.Time);

            switch (interpType) {
                case InterpType.Smooth:
                    interp = interp * interp * (3f - 2f * interp);
                    break;
                case InterpType.EaseIn:
                    interp *= interp;
                    break;
                case InterpType.EaseOut:
                    interp = 1f - interp;
                    interp = 1f - interp * interp;
                    break;
            }

            return Vector4.LerpUnclamped(previous.Value, next.Value, interp);
        }
    }
}
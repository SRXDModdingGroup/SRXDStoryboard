using System.Collections.Generic;
using UnityEngine;

namespace VisualizerSystem.Core; 

public class Curve {
    public double StartTime { get; }
    
    public double EndTime { get; }
    
    public List<Keyframe> Keyframes { get; }

    private List<Vector3> cachedResult = new();

    public Curve(double startTime, double endTime, List<Keyframe> keyframes) {
        StartTime = startTime;
        EndTime = endTime;
        Keyframes = keyframes;
    }

    public List<Vector3> Evaluate(double time, ref int index) {
        if (index < 0)
            index = 0;
        else if (index >= Keyframes.Count - 1)
            index = Keyframes.Count - 2;

        while (true) {
            var previous = Keyframes[index];

            if (time < previous.Time) {
                if (index == 0)
                    return previous.Parameters;
                
                index--;
                
                continue;
            }
            
            var next = Keyframes[index + 1];

            if (time >= next.Time) {
                if (index == Keyframes.Count - 2)
                    return next.Parameters;

                index++;
                
                continue;
            }

            var interpType = previous.InterpType;

            if (interpType == InterpType.Fixed)
                return previous.Parameters;
            
            cachedResult.Clear();

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

            for (int i = 0; i < previous.Parameters.Count && i < next.Parameters.Count; i++)
                cachedResult.Add(Vector3.LerpUnclamped(previous.Parameters[i], next.Parameters[i], interp));

            return cachedResult;
        }
    }
}
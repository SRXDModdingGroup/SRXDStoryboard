using System;
using UnityEngine;

namespace SRXDStoryboard.Core;

public abstract class Curve {
    public abstract void Evaluate(float fromTime, float toTime);
}

public class Curve<T> : Curve {
    public Property<T> Property { get; }
    
    public Keyframe<T>[] Keyframes { get; }

    public Curve(Property<T> property, Keyframe<T>[] keyframes) {
        Property = property;
        Keyframes = keyframes;
    }

    public override void Evaluate(float fromTime, float toTime) {
        int index = Array.BinarySearch(Keyframes, toTime);

        if (index < 0)
            index = ~index;

        if (index == 0) {
            var first = Keyframes[0];
            
            if (fromTime > first.Time)
                Property.Set(first.Value);
            
            return;
        }

        var previous = Keyframes[index - 1];
        var interpType = previous.InterpType;

        if (interpType == InterpType.Fixed || index == Keyframes.Length) {
            if (fromTime < previous.Time || index < Keyframes.Length && fromTime > Keyframes[index].Time)
                Property.Set(previous.Value);
            
            return;
        }

        var next = Keyframes[index];
        float interp = Mathf.InverseLerp(previous.Time, next.Time, toTime);

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
        
        Property.Set(Property.Interp(previous.Value, next.Value, interp));
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem;

internal abstract class Curve {
    public abstract void Evaluate(float time);
}

internal class Curve<T> : Curve {
    private ValueProperty<T> property;
    private Keyframe<T>[] keyframes;
    private int lastEvaluatedIndex = -2;

    public Curve(ValueProperty<T> property, Keyframe<T>[] keyframes) {
        this.property = property;
        this.keyframes = keyframes;
    }

    public override void Evaluate(float time) {
        int index = lastEvaluatedIndex;

        if (index < 0)
            index = 0;

        while (index >= 0 && index < keyframes.Length) {
            if (index < keyframes.Length - 1 && time >= keyframes[index + 1].Time)
                index++;
            else if (time < keyframes[index].Time)
                index--;
            else
                break;
        }
        
        if (index == lastEvaluatedIndex && (index < 0 || index == keyframes.Length - 1 || keyframes[index].InterpType == InterpType.Fixed))
            return;

        lastEvaluatedIndex = index;

        if (index < 0) {
            property.Set(keyframes[0].Value);
            
            return;
        }

        var previous = keyframes[index];
        var interpType = previous.InterpType;

        if (interpType == InterpType.Fixed || index == keyframes.Length - 1) {
            property.Set(previous.Value);
            
            return;
        }

        var next = keyframes[index + 1];
        float interp = Mathf.InverseLerp(previous.Time, next.Time, time);

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
        
        property.Set(property.Interp(previous.Value, next.Value, interp));
    }
}
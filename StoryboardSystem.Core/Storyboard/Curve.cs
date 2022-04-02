using System;
using UnityEngine;

namespace StoryboardSystem.Core;

internal abstract class Curve {
    public abstract void Evaluate(float time);
}

internal class Curve<T> : Curve {
    private Property<T> property;
    private Keyframe<T>[] keyframes;

    public Curve(Property<T> property, Keyframe<T>[] keyframes) {
        this.property = property;
        this.keyframes = keyframes;
    }

    public override void Evaluate(float time) {
        int index = Array.BinarySearch(keyframes, time);

        if (index < 0)
            index = ~index;

        if (index == 0) {
            var first = keyframes[0];
            
            property.Set(first.Value);
            
            return;
        }

        var previous = keyframes[index - 1];
        var interpType = previous.InterpType;

        if (interpType == InterpType.Fixed || index == keyframes.Length) {
            property.Set(previous.Value);
            
            return;
        }

        var next = keyframes[index];
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
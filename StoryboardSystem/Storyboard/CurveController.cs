using System;
using UnityEngine;

namespace StoryboardSystem;

internal class CurveController<T> : TimelineController<T> {
    protected internal override bool IsEvent => false;

    private int lastEvaluatedIndex = -2;
    private Func<T, T, float, T> interpolate;

    public CurveController(Keyframe<T>[] keyframes, Func<T, T, float, T> interpolate) : base(keyframes) => this.interpolate = interpolate;

    protected internal override void Evaluate(float time, Action<T> set) {
        int index = lastEvaluatedIndex;

        if (index < -1)
            index = -1;

        while (true) {
            if (index < Keyframes.Length - 1 && time >= Keyframes[index + 1].Time)
                index++;
            else if (index >= 0 && time < Keyframes[index].Time)
                index--;
            else
                break;
        }
        
        if (index == lastEvaluatedIndex && (index < 0 || index == Keyframes.Length - 1 || Keyframes[index].InterpType == InterpType.Fixed))
            return;

        lastEvaluatedIndex = index;

        if (index < 0) {
            set(Keyframes[0].Value);

            return;
        }

        var previous = Keyframes[index];
        var interpType = previous.InterpType;

        if (interpType == InterpType.Fixed || index == Keyframes.Length - 1) {
            set(previous.Value);
            
            return;
        }

        var next = Keyframes[index + 1];
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

        set(interpolate(previous.Value, next.Value, interp));
    }
}
using UnityEngine;

namespace StoryboardSystem;

internal abstract class Curve {
    public abstract void Evaluate(float time);
}

internal class Curve<T> : Curve {
    private ValueProperty<T>[] properties;
    private Keyframe<T>[] keyframes;
    private int lastEvaluatedIndex = -2;

    public Curve(ValueProperty<T>[] properties, Keyframe<T>[] keyframes) {
        this.properties = properties;
        this.keyframes = keyframes;
    }

    public override void Evaluate(float time) {
        int index = lastEvaluatedIndex;

        if (index < -1)
            index = -1;

        while (true) {
            if (index < keyframes.Length - 1 && time >= keyframes[index + 1].Time)
                index++;
            else if (index >= 0 && time < keyframes[index].Time)
                index--;
            else
                break;
        }
        
        if (index == lastEvaluatedIndex && (index < 0 || index == keyframes.Length - 1 || keyframes[index].InterpType == InterpType.Fixed))
            return;

        lastEvaluatedIndex = index;

        if (index < 0) {
            Set(keyframes[0].Value);
            
            return;
        }

        var previous = keyframes[index];
        var interpType = previous.InterpType;

        if (interpType == InterpType.Fixed || index == keyframes.Length - 1) {
            Set(previous.Value);
            
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
        
        SetInterp(previous.Value, next.Value, interp);
    }

    private void Set(T value) {
        foreach (var property in properties)
            property.Set(value);
    }

    private void SetInterp(T a, T b, float t) {
        foreach (var property in properties)
            property.Set(property.Interp(a, b, t));
    }
}
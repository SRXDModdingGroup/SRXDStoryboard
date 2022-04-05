using System;
using System.Collections.Generic;

namespace StoryboardSystem;

internal abstract class ValueProperty {
    public abstract bool TryCreateCurve(ValueProperty[] properties, List<KeyframeBuilder> keyframeBuilders, ITimeConversion timeConversion, out Curve curve);
}

internal abstract class ValueProperty<T> : ValueProperty {
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);

    public abstract T Interp(T a, T b, float t);

    public override bool TryCreateCurve(ValueProperty[] properties, List<KeyframeBuilder> keyframeBuilders, ITimeConversion timeConversion, out Curve curve) {
        var propertiesT = new ValueProperty<T>[properties.Length];

        for (int i = 0; i < properties.Length; i++) {
            if (properties[i] is not ValueProperty<T> property) {
                curve = null;

                return false;
            }

            propertiesT[i] = property;
        }

        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframeBuilders.Count; i++) {
            if (keyframeBuilders[i].TryCreateKeyframe(this, timeConversion, out keyframes[i]))
                continue;
            
            curve = null;

            return false;
        }

        Array.Sort(keyframes);
        curve = new Curve<T>(propertiesT, keyframes);

        return true;
    }
}
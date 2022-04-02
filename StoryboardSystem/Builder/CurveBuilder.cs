using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class CurveBuilder {
    private List<KeyframeBuilder> keyframeBuilders = new();

    public void AddKey(Timestamp time, VectorN value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public Curve CreateCurve<T>(ValueProperty<T> property, ITimeConversion timeConversion) {
        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframes.Length; i++)
            keyframes[i] = keyframeBuilders[i].CreateKeyframe(property, timeConversion);
        
        Array.Sort(keyframes);

        return new Curve<T>(property, keyframes);
    }
}
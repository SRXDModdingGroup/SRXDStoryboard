using System;
using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal class CurveBuilder {
    private List<KeyframeBuilder> keyframeBuilders = new();

    public void AddKey(Timestamp time, VectorN value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public Curve<T> CreateCurve<T>(Property<T> property, IVectorConversion<T> valueConversion, ITimeConversion conversion) {
        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframes.Length; i++)
            keyframes[i] = keyframeBuilders[i].CreateKeyframe(valueConversion, conversion);
        
        Array.Sort(keyframes);

        return new Curve<T>(property, keyframes);
    }
}
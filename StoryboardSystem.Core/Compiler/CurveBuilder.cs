using System;
using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal abstract class CurveBuilder {
    private static int instanceCounter = 0;
    
    private readonly int instanceId;

    protected CurveBuilder() {
        instanceId = instanceCounter;

        unchecked {
            instanceCounter++;
        }
    }

    public abstract bool TryAddKey(object value, Timestamp time, InterpType interpType, int order);

    public abstract Curve CreateCurve(ITimeConversion conversion);
    
    public override int GetHashCode() => instanceId;
}

internal class CurveBuilder<T> : CurveBuilder {
    private Property<T> property;
    private List<KeyframeBuilder<T>> keyframeBuilders = new();
    public CurveBuilder(Property<T> property) => this.property = property;

    public override bool TryAddKey(object value, Timestamp time, InterpType interpType, int order) {
        if (!Conversion.TryConvert(value, out T cast))
            return false;
        
        keyframeBuilders.Add(new KeyframeBuilder<T>(cast, time, interpType, order));

        return true;
    }

    public override Curve CreateCurve(ITimeConversion conversion) {
        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframes.Length; i++)
            keyframes[i] = keyframeBuilders[i].CreateKeyframe(conversion);
        
        Array.Sort(keyframes);

        return new Curve<T>(property, keyframes);
    }
}
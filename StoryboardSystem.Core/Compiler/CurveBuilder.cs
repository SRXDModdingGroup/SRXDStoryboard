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
    private List<KeyframeBuilder<T>> keyframes = new();
    
    public override bool TryAddKey(object value, Timestamp time, InterpType interpType, int order) {
        if (!Conversion.TryConvert(value, out T cast))
            return false;
        
        keyframes.Add(new KeyframeBuilder<T>(cast, time, interpType, order));

        return true;
    }

    public override Curve CreateCurve(ITimeConversion conversion) {
        
    }
}
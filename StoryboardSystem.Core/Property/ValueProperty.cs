using UnityEngine;

namespace StoryboardSystem.Core;

internal abstract class ValueProperty {
    public abstract Curve CreateCurve(CurveBuilder builder, ITimeConversion timeConversion);
}

internal abstract class ValueProperty<T> : ValueProperty {
    public abstract void Set(T value);

    public abstract T Convert(Vector4 value, int dimensions);

    public abstract T Interp(T a, T b, float t);

    public override Curve CreateCurve(CurveBuilder builder, ITimeConversion timeConversion) => builder.CreateCurve(this, timeConversion);
}
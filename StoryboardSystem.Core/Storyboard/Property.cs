namespace StoryboardSystem.Core;

internal abstract class Property {
    public abstract Curve CreateCurve(CurveBuilder builder, ITimeConversion timeConversion);
}

internal abstract class Property<T> : Property {
    protected abstract IVectorConversion<T> ValueConversion { get; }

    public abstract void Set(T value);

    public abstract T Interp(T a, T b, float t);

    public override Curve CreateCurve(CurveBuilder builder, ITimeConversion timeConversion) => builder.CreateCurve(this, ValueConversion, timeConversion);
}
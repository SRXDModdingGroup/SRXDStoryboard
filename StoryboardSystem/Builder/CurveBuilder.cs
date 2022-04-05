using System.Collections.Generic;

namespace StoryboardSystem; 

internal class CurveBuilder {
    public string Name { get; }
    
    private List<Identifier> bindings = new();
    private List<KeyframeBuilder> keyframeBuilders = new();
    
    public CurveBuilder(string name) => Name = name;

    public void AddBinding(Identifier identifier) {
        if (!bindings.Contains(identifier))
            bindings.Add(identifier);
    }

    public void AddKey(Timestamp time, object value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public bool TryCreateCurve(ITimeConversion conversion, out Curve curve) {
        if (bindings.Count == 0 || keyframeBuilders.Count == 0) {
            curve = null;

            return false;
        }

        var properties = new ValueProperty[bindings.Count];

        for (int i = 0; i < bindings.Count; i++) {
            if (Binder.TryBindValue(bindings[i], out properties[i]))
                continue;
            
            curve = null;

            return false;
        }

        return properties[0].TryCreateCurve(properties, keyframeBuilders, conversion, out curve);
    }
}
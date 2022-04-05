using System.Collections.Generic;

namespace StoryboardSystem; 

internal class TimelineBuilder {
    public string Name { get; }
    
    private List<Identifier> bindings = new();
    private List<KeyframeBuilder> keyframeBuilders = new();
    
    public TimelineBuilder(string name) => Name = name;

    public void AddBinding(Identifier identifier) {
        if (!bindings.Contains(identifier))
            bindings.Add(identifier);
    }

    public void AddKey(Timestamp time, object value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public bool TryCreateTimeline(ITimeConversion conversion, out Timeline timeline) {
        if (bindings.Count == 0 || keyframeBuilders.Count == 0) {
            timeline = null;

            return false;
        }

        var properties = new Property[bindings.Count];

        for (int i = 0; i < bindings.Count; i++) {
            if (Binder.TryBindProperty(bindings[i], out properties[i]))
                continue;
            
            timeline = null;

            return false;
        }

        return properties[0].TryCreateTimeline(properties, keyframeBuilders, conversion, out timeline);
    }
}
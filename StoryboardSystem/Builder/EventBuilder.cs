using System.Collections.Generic;

namespace StoryboardSystem; 

internal class EventBuilder {
    public string Name { get; }
    
    private List<Identifier> bindings = new();
    private List<EventFrameBuilder> eventFrameBuilders = new();
    
    public EventBuilder(string name) => Name = name;

    public void AddBinding(Identifier identifier) {
        if (!bindings.Contains(identifier))
            bindings.Add(identifier);
    }

    public void AddFrame(Timestamp time, object value) => eventFrameBuilders.Add(new EventFrameBuilder(time, value));

    public bool TryCreateEvent(ITimeConversion conversion, out Event @event) {
        if (bindings.Count == 0 || eventFrameBuilders.Count == 0) {
            @event = null;

            return false;
        }

        var properties = new EventProperty[bindings.Count];

        for (int i = 0; i < bindings.Count; i++) {
            if (Binder.TryBindEvent(bindings[i], out properties[i]))
                continue;
            
            @event = null;

            return false;
        }

        return properties[0].TryCreateEvent(properties, eventFrameBuilders, conversion, out @event);
    }
}
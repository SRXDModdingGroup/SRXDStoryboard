using System;
using System.Collections.Generic;

namespace StoryboardSystem;

internal abstract class EventProperty {
    public abstract bool TryCreateEvent(EventProperty[] properties, List<EventFrameBuilder> eventFrameBuilders, ITimeConversion conversion, out Event @event);
}

internal abstract class EventProperty<T> : EventProperty {
    public abstract void Execute(T value);
    
    public abstract bool TryConvert(object value, out T result);

    public override bool TryCreateEvent(EventProperty[] properties, List<EventFrameBuilder> eventFrameBuilders, ITimeConversion conversion, out Event @event) {
        var propertiesT = new EventProperty<T>[properties.Length];

        for (int i = 0; i < properties.Length; i++) {
            if (properties[i] is not EventProperty<T> property) {
                @event = null;

                return false;
            }

            propertiesT[i] = property;
        }
        
        var eventFrames = new EventFrame<T>[eventFrameBuilders.Count];

        for (int i = 0; i < eventFrameBuilders.Count; i++) {
            if (eventFrameBuilders[i].TryCreateEventFrame(this, conversion, out eventFrames[i]))
                continue;

            @event = null;

            return false;
        }

        Array.Sort(eventFrames);
        @event = new Event<T>(propertiesT, eventFrames);

        return true;
    }
}
using System;

namespace StoryboardSystem; 

internal abstract class EventProperty {
    public abstract void Execute();

    public Event[] CreateEvents(EventBuilder builder, ITimeConversion timeConversion) => builder.CreateEvents(this, timeConversion);
}
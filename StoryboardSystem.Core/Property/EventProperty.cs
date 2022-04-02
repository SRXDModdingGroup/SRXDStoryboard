using System;

namespace StoryboardSystem.Core; 

internal abstract class EventProperty {
    public abstract void Execute();

    public Event[] CreateEvents(EventBuilder builder, ITimeConversion timeConversion) => builder.CreateEvents(this, timeConversion);
}
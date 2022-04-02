using System;

namespace StoryboardSystem.Core; 

internal readonly struct Event : IComparable<Event> {
    private readonly float time;
    private readonly EventProperty property;

    public Event(float time, EventProperty property) {
        this.time = time;
        this.property = property;
    }

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime < time && toTime >= time)
            property.Execute();
    }

    public int CompareTo(Event other) => time.CompareTo(other.time);
}
using System;

namespace StoryboardSystem.Core; 

internal readonly struct Event : IComparable<Event> {
    private readonly float time;
    private readonly Action execute;

    public Event(float time, Action execute) {
        this.time = time;
        this.execute = execute;
    }

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime < time && toTime >= time)
            execute();
    }

    public int CompareTo(Event other) => time.CompareTo(other.time);
}
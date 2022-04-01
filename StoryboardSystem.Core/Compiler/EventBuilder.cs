using System;
using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal class EventBuilder {
    private static int instanceCounter = 0;

    private Action execute;
    private List<Timestamp> times = new();
    private readonly int instanceId;

    public EventBuilder(Action execute) {
        this.execute = execute;
        instanceId = instanceCounter;

        unchecked {
            instanceCounter++;
        }
    }

    public void AddTime(Timestamp timestamp) => times.Add(timestamp);

    public Event[] CreateEvents(ITimeConversion conversion) {
        var events = new Event[times.Count];

        for (int i = 0; i < events.Length; i++) {
            var time = times[i];

            events[i] = new Event(conversion.Convert(time.Beats, time.Ticks, time.Seconds), execute);
        }
        
        Array.Sort(events);

        return events;
    }

    public override int GetHashCode() => instanceId;
}
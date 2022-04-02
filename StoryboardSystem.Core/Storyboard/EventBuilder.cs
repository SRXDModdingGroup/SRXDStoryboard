using System;
using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal class EventBuilder {
    private List<Timestamp> times = new();

    public void AddTime(Timestamp timestamp) => times.Add(timestamp);

    public Event[] CreateEvents(Action execute, ITimeConversion conversion) {
        var events = new Event[times.Count];

        for (int i = 0; i < events.Length; i++) {
            var time = times[i];

            events[i] = new Event(conversion.Convert(time.Beats, time.Ticks, time.Seconds), execute);
        }
        
        Array.Sort(events);

        return events;
    }
}
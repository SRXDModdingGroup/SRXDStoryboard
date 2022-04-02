using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class EventBuilder {
    private List<Timestamp> times = new();

    public void AddTime(Timestamp timestamp) => times.Add(timestamp);

    public Event[] CreateEvents(EventProperty property, ITimeConversion conversion) {
        var events = new Event[times.Count];

        for (int i = 0; i < events.Length; i++) {
            var time = times[i];

            events[i] = new Event(conversion.Convert(time.Beats, time.Ticks, time.Seconds), property);
        }
        
        Array.Sort(events);

        return events;
    }
}
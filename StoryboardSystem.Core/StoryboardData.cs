using System.Collections.Generic;

namespace StoryboardSystem.Core;

public class StoryboardData {
    public List<(string rigName, int rigIndex, string eventName, List<EventCall> eventCalls)> EventCalls { get; }
    
    public List<(string rigName, int rigIndex, string property, List<Curve> curves)> Curves { get; }

    public StoryboardData(List<(string rigName, int rigIndex, string eventName, List<EventCall>)> eventCalls, List<(string rigName, int rigIndex, string property, List<Curve>)> curves) {
        EventCalls = eventCalls;
        Curves = curves;
    }
}

using System.Collections.Generic;

namespace StoryboardSystem.Core;

public class StoryboardData {
    public List<RigReference<List<EventCall>>> EventCalls { get; }
    
    public List<RigReference<List<Curve>>> Curves { get; }

    public StoryboardData(List<RigReference<List<EventCall>>> eventCalls, List<RigReference<List<Curve>>> curves) {
        EventCalls = eventCalls;
        Curves = curves;
    }
}

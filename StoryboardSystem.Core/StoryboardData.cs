using System.Collections.Generic;

namespace StoryboardSystem.Core;

public class StoryboardData {
    public List<RigReference<List<Keyframe>>> EventCalls { get; }
    
    public List<RigReference<List<Curve>>> Curves { get; }

    public StoryboardData(List<RigReference<List<Keyframe>>> eventCalls, List<RigReference<List<Curve>>> curves) {
        EventCalls = eventCalls;
        Curves = curves;
    }
}

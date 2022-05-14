using System.Collections.Generic;

namespace StoryboardSystem.Core; 

public class EventController {
    public EventBinding Binding { get; }
    
    public List<EventCall> Calls { get; }

    private int lastCallIndex;

    public EventController(EventBinding binding, List<EventCall> calls) {
        Binding = binding;
        Calls = calls;
    }

    public void Evaluate(double time, bool trigger) {
        while (true) {
            if (lastCallIndex < Calls.Count - 1 && time >= Calls[lastCallIndex + 1].Time) {
                lastCallIndex++;

                if (!trigger)
                    continue;
                
                Binding.Execute(Calls[lastCallIndex].Parameters);
            }
            else if (lastCallIndex >= 0 && time < Calls[lastCallIndex].Time)
                lastCallIndex--;
            else
                return;
        }
    }
}
namespace SRXDStoryboard.Plugin; 

public class Storyboard {
    public Event[] Events { get; }
    
    public IEvaluable[] Evaluables { get; }

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime == toTime)
            return;

        foreach (var evaluable in Evaluables)
            evaluable.Evaluate(fromTime, toTime);
    }
}
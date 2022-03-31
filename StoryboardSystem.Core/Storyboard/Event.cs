namespace StoryboardSystem.Core; 

internal abstract class Event {
    public float Time { get; }

    public abstract void Execute();

    protected Event(float time) => Time = time;
    
    public void Evaluate(float fromTime, float toTime) {
        if (fromTime < Time && toTime >= Time)
            Execute();
    }
}
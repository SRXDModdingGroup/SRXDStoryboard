namespace StoryboardSystem;

internal abstract class Event {
    public abstract void Evaluate(float time);
}

internal class Event<T> : Event {
    private EventProperty<T>[] properties;
    private EventFrame<T>[] eventFrames;
    private int index = -1;

    public Event(EventProperty<T>[] properties, EventFrame<T>[] eventFrames) {
        this.properties = properties;
        this.eventFrames = eventFrames;
    }

    public override void Evaluate(float time) {
        while (true) {
            if (index < eventFrames.Length - 1 && time >= eventFrames[index + 1].Time) {
                index++;
                Execute(eventFrames[index].Value);
            }
            else if (index >= 0 && time < eventFrames[index].Time)
                index--;
            else
                break;
        }
    }

    private void Execute(T value) {
        foreach (var property in properties)
            property.Execute(value);
    }
}
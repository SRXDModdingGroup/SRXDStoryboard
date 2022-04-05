namespace StoryboardSystem;

internal class Event<T> : Timeline<T> {
    private int index = -1;

    
    public Event(Property<T>[] properties, Keyframe<T>[] keyframes) : base(properties, keyframes) { }
    
    public override void Evaluate(float time) {
        while (true) {
            if (index < Keyframes.Length - 1 && time >= Keyframes[index + 1].Time) {
                index++;
                Set(Keyframes[index].Value);
            }
            else if (index >= 0 && time < Keyframes[index].Time)
                index--;
            else
                break;
        }
    }
}
using System;

namespace StoryboardSystem;

internal class EventController<T> : TimelineController<T> {
    protected internal override bool IsEvent => true;

    private int index = -1;

    public EventController(Keyframe<T>[] keyframes) : base(keyframes) { }

    protected internal override void Evaluate(float time, Action<T> set) {
        while (true) {
            if (index < Keyframes.Length - 1 && time >= Keyframes[index + 1].Time) {
                index++;
                set(Keyframes[index].Value);
            }
            else if (index >= 0 && time < Keyframes[index].Time)
                index--;
            else
                break;
        }
    }
}
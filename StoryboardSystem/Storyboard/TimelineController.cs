using System;

namespace StoryboardSystem;

internal abstract class TimelineController<T> : IController<T> {
    protected Keyframe<T>[] Keyframes { get; }

    protected TimelineController(Keyframe<T>[] keyframes) => Keyframes = keyframes;
    
    public abstract void Evaluate(float time, Action<T> set);
}
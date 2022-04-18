using System;

namespace StoryboardSystem;

internal abstract class TimelineController : Controller { }

internal abstract class TimelineController<T> : TimelineController {
    protected Keyframe<T>[] Keyframes { get; }

    protected TimelineController(Keyframe<T>[] keyframes) => Keyframes = keyframes;
    
    public abstract void Evaluate(float time, Action<T> set);
}
using System;

namespace StoryboardSystem;

internal abstract class TimelineController<T> : Controller<T> {
    protected Keyframe<T>[] Keyframes { get; }

    protected TimelineController(Keyframe<T>[] keyframes) => Keyframes = keyframes;
}
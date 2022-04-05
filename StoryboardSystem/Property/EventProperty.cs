using System;

namespace StoryboardSystem;

internal abstract class EventProperty : Property<object> {
    public override void Set(object value) => Execute();

    protected override bool Validate(Property<object> property) => property is EventProperty<object>;

    protected override Timeline CreateTimeline(Property<object>[] properties, Keyframe<object>[] keyframes) => new Event<object>(properties, keyframes);

    protected abstract void Execute();
}

internal abstract class EventProperty<T> : Property<T> {
    public override void Set(T value) => Execute(value);

    protected override bool Validate(Property<T> property) => property is EventProperty<T>;

    protected override Timeline CreateTimeline(Property<T>[] properties, Keyframe<T>[] keyframes) => new Event<T>(properties, keyframes);

    protected abstract void Execute(T value);
}
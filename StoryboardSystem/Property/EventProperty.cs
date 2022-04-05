using System;

namespace StoryboardSystem;

internal abstract class EventProperty : Property<object> {
    public override void Set(object value) => Execute();

    protected abstract void Execute();
}

internal abstract class EventProperty<T> : Property<T> {
    public override void Set(T value) => Execute(value);

    protected abstract void Execute(T value);
}
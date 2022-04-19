namespace StoryboardSystem;

internal abstract class ValueProperty<T> : Property<T> {
    protected internal override bool IsEvent => false;
}
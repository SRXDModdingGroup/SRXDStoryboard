namespace StoryboardSystem;

internal abstract class ValueProperty<T> : Property<T> {
    public override bool IsEvent => false;
}
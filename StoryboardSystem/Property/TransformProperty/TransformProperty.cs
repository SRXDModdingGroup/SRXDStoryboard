using UnityEngine;

namespace StoryboardSystem; 

internal abstract class TransformProperty<T> : ValueProperty<T> {
    protected Transform Transform { get; }

    protected TransformProperty(Transform transform) => Transform = transform;
}
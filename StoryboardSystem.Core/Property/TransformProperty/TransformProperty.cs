using UnityEngine;

namespace StoryboardSystem.Core; 

internal abstract class TransformProperty<T> : ValueProperty<T> {
    protected Transform Transform { get; }

    protected TransformProperty(Transform transform) => this.Transform = transform;
}
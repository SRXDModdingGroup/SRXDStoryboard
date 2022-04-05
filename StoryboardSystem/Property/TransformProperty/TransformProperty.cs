using UnityEngine;

namespace StoryboardSystem; 

internal abstract class TransformProperty<T> : VectorProperty<T> {
    protected Transform Transform { get; }

    protected TransformProperty(Transform transform) => Transform = transform;
}
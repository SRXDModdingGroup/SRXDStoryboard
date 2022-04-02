using UnityEngine;

namespace StoryboardSystem.Core; 

internal class PositionProperty : TransformProperty<Vector3> {
    public PositionProperty(Transform transform) : base(transform) { }
    
    public override void Set(Vector3 value) => Transform.localPosition = value;
    
    public override Vector3 Convert(Vector4 value, int dimensions) => new(value.x, value.y, value.z);

    public override Vector3 Interp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
}
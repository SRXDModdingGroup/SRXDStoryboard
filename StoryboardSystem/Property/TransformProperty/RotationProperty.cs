using UnityEngine;

namespace StoryboardSystem; 

internal class RotationProperty : TransformProperty<Vector3> {
    private Quaternion defaultRotation;

    public RotationProperty(Transform transform) : base(transform) => defaultRotation = transform.localRotation;

    protected internal override void Reset() => Transform.localRotation = defaultRotation;

    protected internal override void Set(Vector3 value) => Transform.localRotation = Quaternion.Euler(value);

    protected internal override Vector3 Interpolate(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector3 result) {
        switch (dimensions) {
            case 1:
                result = new Vector3(0f, 0f, value.x);
                return true;
            case 3:
                result = new Vector3(value.x, value.y, value.z);
                return true;
            default:
                result = Vector3.zero;
                return false;
        }
    }
}
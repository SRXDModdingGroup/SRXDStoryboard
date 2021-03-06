using UnityEngine;

namespace StoryboardSystem; 

internal class PositionProperty : TransformProperty<Vector3> {
    private Vector3 defaultPosition;

    public PositionProperty(Transform transform) : base(transform) => defaultPosition = transform.localPosition;

    protected internal override void Reset() => Transform.localPosition = defaultPosition;

    protected internal override void Set(Vector3 value) => Transform.localPosition = value;

    protected internal override Vector3 Interpolate(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector3 result) {
        switch (dimensions) {
            case 2:
                result = new Vector3(value.x, value.y, 0f);
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
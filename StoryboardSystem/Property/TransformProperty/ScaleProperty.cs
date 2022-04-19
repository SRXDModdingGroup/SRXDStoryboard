using UnityEngine;

namespace StoryboardSystem; 

internal class ScaleProperty : TransformProperty<Vector3> {
    private Vector3 defaultScale;

    public ScaleProperty(Transform transform) : base(transform) => defaultScale = transform.localScale;

    protected internal override void Reset() => Transform.localScale = defaultScale;

    protected internal override void Set(Vector3 value) => Transform.localScale = value;

    protected internal override Vector3 Interpolate(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector3 result) {
        switch (dimensions) {
            case 1:
                float x = value.x;
                
                result = new Vector3(x, x, x);
                return true;
            case 2:
                result = new Vector3(value.x, value.y, 1f);
                return true;
            case 3:
                result = new Vector3(value.x, value.y, value.z);
                return true;
            default:
                result = Vector3.one;
                return false;
        }
    }
}
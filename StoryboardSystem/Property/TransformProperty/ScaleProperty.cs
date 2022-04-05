using UnityEngine;

namespace StoryboardSystem; 

internal class ScaleProperty : TransformProperty<Vector3> {
    public ScaleProperty(Transform transform) : base(transform) { }
    
    public override void Set(Vector3 value) => Transform.localScale = value;
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector3 result) {
        if (dimensions > 3) {
            result = Vector3.zero;

            return false;
        }

        result = dimensions switch {
            1 => new Vector3(value.x, value.x, value.x),
            2 => new Vector3(value.x, value.y, 1f),
            _ => new Vector3(value.x, value.y, value.z)
        };

        return true;
    }

    public override Vector3 Interp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
}
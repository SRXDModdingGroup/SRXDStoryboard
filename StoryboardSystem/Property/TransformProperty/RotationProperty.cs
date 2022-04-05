using UnityEngine;

namespace StoryboardSystem; 

internal class RotationProperty : TransformProperty<Quaternion> {
    public RotationProperty(Transform transform) : base(transform) { }
    
    public override void Set(Quaternion value) => Transform.rotation = value;

    public override Quaternion Interp(Quaternion a, Quaternion b, float t) => Quaternion.Slerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Quaternion result) {
        if (dimensions == 2) {
            result = Quaternion.identity;

            return false;
        }

        result = dimensions switch {
            1 => Quaternion.Euler(0f, 0f, value.x),
            3 => Quaternion.Euler(value.x, value.y, value.z),
            _ => new Quaternion(value.x, value.y, value.z, value.w)
        };

        return true;
    }
}
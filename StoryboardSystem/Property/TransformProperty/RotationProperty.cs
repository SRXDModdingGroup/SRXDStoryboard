using UnityEngine;

namespace StoryboardSystem; 

internal class RotationProperty : TransformProperty<Quaternion> {
    public RotationProperty(Transform transform) : base(transform) { }
    
    public override void Set(Quaternion value) => Transform.rotation = value;
    
    public override Quaternion Convert(Vector4 value, int dimensions) {
        if (dimensions == 4)
            return new Quaternion(value.x, value.y, value.z, value.w);
        
        return Quaternion.Euler(value.x, value.y, value.z);
    }

    public override Quaternion Interp(Quaternion a, Quaternion b, float t) => Quaternion.Slerp(a, b, t);
}
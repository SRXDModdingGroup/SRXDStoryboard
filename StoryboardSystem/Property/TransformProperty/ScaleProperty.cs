using UnityEngine;

namespace StoryboardSystem; 

internal class ScaleProperty : TransformProperty<Vector3> {
    public ScaleProperty(Transform transform) : base(transform) { }
    
    public override void Set(Vector3 value) => Transform.localScale = value;
    
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

    protected override Vector3 Interp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
}
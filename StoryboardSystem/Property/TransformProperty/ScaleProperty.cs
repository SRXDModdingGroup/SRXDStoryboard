using UnityEngine;

namespace StoryboardSystem; 

internal class ScaleProperty : TransformProperty<Vector3> {
    public ScaleProperty(Transform transform) : base(transform) { }
    
    public override void Set(Vector3 value) => Transform.localScale = value;
    
    public override Vector3 Convert(Vector4 value, int dimensions) {
        if (dimensions > 1)
            return new Vector3(value.x, value.y, value.z);
        
        float scale = value.x;

        return new Vector3(scale, scale, scale);
    }

    public override Vector3 Interp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
}
using UnityEngine;

namespace StoryboardSystem; 

internal class MaterialVectorProperty : MaterialProperty<Vector4> {
    public MaterialVectorProperty(Material material, int id) : base(material, id) { }

    public override void Set(Vector4 value) => Material.SetVector(Id, value);

    public override Vector4 Interp(Vector4 a, Vector4 b, float t) => Vector4.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector4 result) {
        result = value;

        return true;
    }
}
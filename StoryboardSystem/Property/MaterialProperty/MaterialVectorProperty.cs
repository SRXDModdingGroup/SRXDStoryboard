using UnityEngine;

namespace StoryboardSystem; 

internal class MaterialVectorProperty : MaterialProperty<Vector4> {
    private Vector4 defaultValue;

    public MaterialVectorProperty(Material material, int id) : base(material, id) {
        if (material.HasVector(id))
            defaultValue = material.GetVector(id);
    }

    protected internal override void Reset() => Material.SetVector(Id, defaultValue);

    protected internal override void Set(Vector4 value) => Material.SetVector(Id, value);

    protected internal override Vector4 Interpolate(Vector4 a, Vector4 b, float t) => Vector4.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Vector4 result) {
        result = value;

        return true;
    }
}
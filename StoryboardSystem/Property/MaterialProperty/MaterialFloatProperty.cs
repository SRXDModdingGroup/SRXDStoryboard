using UnityEngine;

namespace StoryboardSystem; 

internal class MaterialFloatProperty : MaterialProperty<float> {
    private float defaultValue;

    public MaterialFloatProperty(Material material, int id) : base(material, id) {
        if (material.HasFloat(id))
            defaultValue = material.GetFloat(id);
    }

    protected internal override void Reset() => Material.SetFloat(Id, defaultValue);

    protected internal override void Set(float value) => Material.SetFloat(Id, value);

    protected internal override float Interpolate(float a, float b, float t) => Mathf.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out float result) {
        if (dimensions > 1) {
            result = 0f;

            return false;
        }

        result = value.x;

        return true;
    }
}
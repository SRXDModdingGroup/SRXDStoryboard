using UnityEngine;

namespace StoryboardSystem; 

internal class MaterialFloatProperty : MaterialProperty<float> {
    public MaterialFloatProperty(Material material, int id) : base(material, id) { }

    public override void Set(float value) => Material.SetFloat(Id, value);

    public override float Interp(float a, float b, float t) => Mathf.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out float result) {
        if (dimensions > 1) {
            result = 0f;

            return false;
        }

        result = value.x;

        return true;
    }
}
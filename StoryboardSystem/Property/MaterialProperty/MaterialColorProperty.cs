using UnityEngine;

namespace StoryboardSystem; 

internal class MaterialColorProperty : MaterialProperty<Color> {
    private Color defaultColor;
    
    public MaterialColorProperty(Material material, int id) : base(material, id) {
        if (material.HasColor(id))
            defaultColor = material.GetColor(id);
    }

    protected internal override void Reset() => Material.SetColor(Id, defaultColor);

    protected internal override void Set(Color value) => Material.SetColor(Id, value);

    protected internal override Color Interpolate(Color a, Color b, float t) => Color.Lerp(a, b, t);
    
    protected override bool TryConvert(Vector4 value, int dimensions, out Color result) {
        switch (dimensions) {
            case 1: {
                float x = value.x;

                result = new Color(x, x, x);

                return true;
            }
            case 2: {
                float x = value.x;
                float y = value.y;

                result = new Color(x, x, x, y);

                return true;
            }
            case 3:
                result = new Color(value.x, value.y, value.z);

                return true;
            default:
                result = new Color(value.x, value.y, value.z, value.w);

                return true;
        }
    }
}
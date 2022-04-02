using UnityEngine;

namespace StoryboardSystem.Core; 

internal class MaterialColorProperty : MaterialProperty<Color> {
    public MaterialColorProperty(Material material, int id) : base(material, id) { }

    public override void Set(Color value) => Material.SetColor(Id, value);

    public override Color Convert(Vector4 value, int dimensions) {
        switch (dimensions) {
            case 1: {
                float x = value.x;

                return new Color(x, x, x, 1f);
            }
            case 2: {
                float x = value.x;
                float y = value.y;

                return new Color(x, x, x, y);
            }
            case 3:
                return new Color(value.x, value.y, value.z);
            default:
                return new Color(value.x, value.y, value.z, value.w);
        }
    }

    public override Color Interp(Color a, Color b, float t) => Color.Lerp(a, b, t);
}
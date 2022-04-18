using UnityEngine;

namespace StoryboardSystem; 

internal class CameraFovProperty : ValueProperty<float> {
    private Camera camera;
    
    public CameraFovProperty(Camera camera) => this.camera = camera;

    public override void Set(float value) => camera.fieldOfView = value;

    public override bool TryConvert(object value, out float result) {
        switch (value) {
            case float floatVal:
                result = floatVal;
                return true;
            case int intVal:
                result = intVal;
                return true;
            default:
                result = 0f;
                return false;
        }
    }

    public override float Interpolate(float a, float b, float t) => Mathf.Lerp(a, b, t);
}
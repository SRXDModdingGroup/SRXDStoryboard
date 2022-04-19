using UnityEngine;

namespace StoryboardSystem; 

internal class CameraFovProperty : ValueProperty<float> {
    private Camera camera;
    private float defaultFov;
    
    public CameraFovProperty(Camera camera) {
        this.camera = camera;
        defaultFov = camera.fieldOfView;
    }

    protected internal override void Reset() => camera.fieldOfView = defaultFov;

    protected internal override void Set(float value) => camera.fieldOfView = value;

    protected internal override bool TryConvert(object value, out float result) {
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

    protected internal override float Interpolate(float a, float b, float t) => Mathf.Lerp(a, b, t);
}
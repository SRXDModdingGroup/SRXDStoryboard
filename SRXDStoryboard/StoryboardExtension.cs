using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public class StoryboardExtension : IStoryboardExtension {
    public object GetExternalObject(string name) => name switch {
        "StaticRoot" => Track.Instance.cameraContainerTransform.Find("StaticRoot"),
        "CameraRoot" => MainCamera.Instance.transform,
        "CameraManipulator" => Track.Instance.cameraContainerTransform.Find("Manipulator"),
        "ForegroundCamera" => MainCamera.Instance.GetComponent<Camera>(),
        "BackgroundCamera" => MainCamera.Instance.backgroundCamera,
        _ => null
    };
}
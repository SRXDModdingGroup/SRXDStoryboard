using System.Collections.Generic;
using SMU;
using SRXDPostProcessing;
using StoryboardSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDStoryboard; 

public class SceneManager : ISceneManager {
    private string customAssetBundlePath;
    private Dictionary<PostProcessingInfo, PostProcessingInstance> postProcessingInstances = new();
    private float[] beatArray;
    private TimeSignatureSegment[] segments;

    public SceneManager(string customAssetBundlePath, PlayableTrackData trackData) {
        this.customAssetBundlePath = customAssetBundlePath;
        beatArray = trackData.BeatArray;
        segments = trackData.TimeSignatureSegments;
    }

    public void Update(float time, bool triggerEvents) { }

    public void Start(Storyboard storyboard) {
        var mainCamera = MainCamera.Instance;
        var camera = mainCamera.GetComponent<Camera>();

        if (storyboard.TryGetOutParam("FarClip", out int farClip))
            camera.farClipPlane = farClip;

        if (storyboard.TryGetOutParam("ForegroundDepth", out bool foregroundDepth) && foregroundDepth)
            camera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
        
        if (storyboard.TryGetOutParam("BackgroundColor", out bool backgroundColor) && backgroundColor)
            mainCamera.backgroundCamera.GetUniversalAdditionalCameraData().requiresColorTexture = true;

        if (storyboard.TryGetOutParam("BackgroundDepth", out bool backgroundDepth) && backgroundDepth)
            mainCamera.backgroundCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
    }

    public void Stop(Storyboard storyboard) {
        var mainCamera = MainCamera.Instance;
        var camera = mainCamera.GetComponent<Camera>();
        var cameraData = camera.GetUniversalAdditionalCameraData();

        camera.farClipPlane = 100f;
        camera.fieldOfView = 93.5f;
        cameraData.requiresDepthTexture = false;
        cameraData = mainCamera.backgroundCamera.GetUniversalAdditionalCameraData();
        cameraData.requiresColorTexture = false;
        cameraData.requiresDepthTexture = false;

        var cameraManipulator = Track.Instance.cameraContainerTransform.Find("Manipulator");
        
        cameraManipulator.localPosition = Vector3.zero;
        cameraManipulator.localRotation = Quaternion.identity;
    }

    public void AddPostProcessingInstance(Material material, Camera targetCamera) {
        var info = new PostProcessingInfo(material, targetCamera);
        
        if (postProcessingInstances.ContainsKey(info))
            return;

        var instance = new PostProcessingInstance(material, targetCamera);
        
        PostProcessingManager.AddPostProcessingInstance(instance);
        postProcessingInstances.Add(info, instance);
    }

    public void RemovePostProcessingInstance(Material material, Camera targetCamera) {
        var info = new PostProcessingInfo(material, targetCamera);
        
        if (!postProcessingInstances.TryGetValue(info, out var instance))
            return;
        
        PostProcessingManager.RemovePostProcessingInstance(instance);
        postProcessingInstances.Remove(info);
    }

    public void SetPostProcessingInstanceEnabled(Material material, Camera targetCamera, bool enabled) {
        if (postProcessingInstances.TryGetValue(new PostProcessingInfo(material, targetCamera), out var instance))
            instance.Enabled = enabled;
    }

    public void UnloadAssetBundle(string bundleName) => AssetBundleUtility.UnloadAssetBundle(customAssetBundlePath, bundleName);

    public bool TryGetAssetBundle(string bundleName, out AssetBundle bundle)
        => AssetBundleUtility.TryGetAssetBundle(customAssetBundlePath, bundleName, out bundle);
    
    public bool TryGetExternalObject(string name, out object externalObject) {
        switch (name) {
            case "StaticRoot":
                externalObject = Track.Instance.cameraContainerTransform.Find("StaticRoot");
                return true;
            case "CameraRoot":
                externalObject = MainCamera.Instance.transform;
                return true;
            case "CameraManipulator":
                externalObject = Track.Instance.cameraContainerTransform.Find("Manipulator");
                return true;
            case "ForegroundCamera":
                externalObject = MainCamera.Instance.GetComponent<Camera>();
                return true;
            case "BackgroundCamera":
                externalObject = MainCamera.Instance.backgroundCamera;
                return true;
            default:
                externalObject = null;
                return false;
        }
    }

    public float Convert(float measures, float beats, float ticks, float seconds) {
        float beat = beats + 0.125f * ticks;
        
        if (segments.Length > 0) {
            int index0 = 0;

            while (index0 < segments.Length - 1 && segments[index0 + 1].startingBar <= measures)
                index0++;

            var segment = segments[index0];
            
            beat += segments[index0].startingBeat + (measures - segment.startingBar) * segment.ticksPerBar * segment.beatsPerTick;
        }
         
        int index1 = Mathf.Clamp(Mathf.FloorToInt(beat), 0, beatArray.Length - 2);
        float time = Mathf.LerpUnclamped(beatArray[index1], beatArray[index1 + 1], beat - index1) + seconds;
        
        return time;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace StoryboardSystem; 

public class Storyboard {
    public bool HasData { get; private set; }

    private bool active;
    private bool opened;
    private bool shouldOpenOnRecompile;
    private float lastTime;
    private string name;
    private string directory;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private LoadedExternalObjectReference[] externalObjectReferences;
    private CameraIdentifier[] cameraIdentifiers;
    private List<TimelineBuilder> timelineBuilders;
    private Dictionary<string, object> outParams;
    private Binding[] bindings;
    private Camera[] cameras;
    private Camera previousBaseCamera;
    private Camera[] previousCameraStack;

    internal Storyboard(
        string name,
        string directory) {
        this.name = name;
        this.directory = directory;
    }
    
    public bool TryGetOutParam<T>(string name, out T value) {
        if (outParams != null && outParams.TryGetValue(name, out object obj) && obj is T cast) {
            value = cast;

            return true;
        }

        value = default;

        return false;
    }

    internal void Play() {
        active = true;

        if (opened) {
            foreach (var reference in postProcessReferences)
                reference.SetStoryboardActive(true);
        }
        
        Evaluate(lastTime, false);
    }

    internal void Stop() {
        active = false;

        if (!opened)
            return;
            
        foreach (var reference in postProcessReferences)
            reference.SetStoryboardActive(false);
    }

    internal void Evaluate(float time, bool triggerEvents) {
        lastTime = time;
        
        if (!opened || !active)
            return;

        foreach (var binding in bindings) {
            if (triggerEvents || !binding.IsEvent)
                binding.Evaluate(time);
        }
    }

    internal void Compile(ISceneManager sceneManager, ILogger logger, bool force = false) {
        if (HasData && !force)
            return;
        
        ClearData(sceneManager);
        
        if (Compiler.TryCompileFile(name, directory, logger, out var result))
            SetData(sceneManager, result);
    }

    internal void Recompile(bool force, IAssetBundleManager assetBundleManager, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        if (HasData && !force)
            return;
        
        Compile(sceneManager, logger, force);

        if (shouldOpenOnRecompile)
            Open(assetBundleManager, sceneManager, storyboardParams, logger);
    }

    internal void Open(IAssetBundleManager assetBundleManager, ISceneManager sceneManager, IStoryboardParams storyboardParams, ILogger logger) {
        Close(sceneManager);
        shouldOpenOnRecompile = true;
        
        if (!HasData)
            return;

        bool success = true;
        var watch = Stopwatch.StartNew();
        
        foreach (var reference in assetBundleReferences)
            success = reference.TryLoad(assetBundleManager, logger) && success;
        
        foreach (var reference in assetReferences)
            success = reference.TryLoad(logger) && success;
        
        foreach (var reference in instanceReferences)
            success = reference.TryLoad(sceneManager, logger) && success;

        var camerasList = new List<Camera>(sceneManager.Cameras);
        UniversalAdditionalCameraData previousBaseCameraData = null;

        previousBaseCamera = null;

        foreach (var camera in camerasList) {
            if (!camera.TryGetComponent(out previousBaseCameraData) || previousBaseCameraData.renderType != CameraRenderType.Base)
                continue;
            
            previousBaseCamera = camera;

            break;
        }

        previousCameraStack = null;

        if (previousBaseCamera != null && previousBaseCameraData != null)
            previousCameraStack = previousBaseCameraData.cameraStack.ToArray();

        foreach (var identifier in cameraIdentifiers) {
            if (!Binder.TryResolveIdentifier(identifier.Identifier, out object obj) || obj is not Camera camera) {
                logger.LogWarning($"Failed to open {name}: {identifier} is not a camera");
                success = false;
                
                continue;
            }
            
            camerasList.Insert(identifier.Index, camera);
        }

        cameras = camerasList.ToArray();

        if (cameras.Length > 0) {
            if (cameras.Length > 0 && cameras[0].TryGetComponent<UniversalAdditionalCameraData>(out var data0)) {
                data0.renderType = CameraRenderType.Base;
                data0.cameraStack.Clear();
                data0.cameraStack.AddRange(cameras.Skip(1));
            }

            for (int i = 1; i < cameras.Length; i++) {
                if (cameras[i].TryGetComponent<UniversalAdditionalCameraData>(out var data1))
                    data1.renderType = CameraRenderType.Overlay;
            }
        }
        else {
            logger.LogWarning($"Failed to open {name}: No cameras found");

            success = false;
        }

        foreach (var reference in postProcessReferences)
            success = reference.TryLoad(cameras, sceneManager, logger) && success;

        foreach (var reference in externalObjectReferences)
            success = reference.TryLoad(storyboardParams, logger) && success;

        if (!success) {
            Close(sceneManager);
            
            return;
        }

        bindings = new Binding[timelineBuilders.Count];

        for (int i = 0; i < timelineBuilders.Count; i++) {
            if (timelineBuilders[i].TryCreateBinding(storyboardParams, logger, out var binding)) {
                bindings[i] = binding;
                
                continue;
            }
            
            logger.LogWarning($"Failed to open {name}: Could not create timeline for {timelineBuilders[i].Name}");
            success = false;
        }

        if (!success) {
            Close(sceneManager);

            return;
        }
        
        if (active)
            Play();
        else
            Stop();

        opened = true;
        watch.Stop();
        logger.LogMessage($"Successfully opened {name} in {watch.ElapsedMilliseconds}ms");
    }

    internal void Close(ISceneManager sceneManager, bool clearOpenOnRecompile = false) {
        opened = false;
        bindings = null;

        if (clearOpenOnRecompile)
            shouldOpenOnRecompile = false;

        if (!HasData)
            return;
        
        foreach (var reference in externalObjectReferences)
            reference.Unload();

        foreach (var reference in postProcessReferences)
            reference.Unload();

        foreach (var camera in sceneManager.Cameras) {
            if (!camera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
                continue;
            
            data.cameraStack.Clear();

            if (camera == previousBaseCamera) {
                data.renderType = CameraRenderType.Base;
                data.cameraStack.AddRange(previousCameraStack);
            }
            else
                data.renderType = CameraRenderType.Overlay;
        }

        cameras = null;

        foreach (var reference in instanceReferences)
            reference.Unload();

        foreach (var reference in assetReferences)
            reference.Unload();

        foreach (var reference in assetBundleReferences)
            reference.Unload();
    }

    private void SetData(ISceneManager sceneManager, StoryboardData data) {
        Close(sceneManager);
        assetBundleReferences = data.AssetBundleReferences;
        assetReferences = data.AssetReferences;
        instanceReferences = data.InstanceReferences;
        postProcessReferences = data.PostProcessReferences;
        externalObjectReferences = data.ExternalObjectReferences;
        cameraIdentifiers = data.CameraIdentifiers;
        timelineBuilders = data.TimelineBuilders;
        outParams = data.OutParams;
        HasData = true;
    }

    private void ClearData(ISceneManager sceneManager) {
        Close(sceneManager);
        assetBundleReferences = null;
        assetReferences = null;
        instanceReferences = null;
        postProcessReferences = null;
        externalObjectReferences = null;
        cameraIdentifiers = null;
        timelineBuilders = null;
        outParams = null;
        HasData = false;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SMU.Utilities;
using StoryboardSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDStoryboard; 

public static class Patches {
    private static void ApplyOutParams(Storyboard storyboard) {
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
    
    private static void ResetCameraSettings() {
        var mainCamera = MainCamera.Instance;
        var camera = mainCamera.GetComponent<Camera>();
        var cameraData = camera.GetUniversalAdditionalCameraData();

        camera.farClipPlane = 100f;
        cameraData.requiresDepthTexture = false;
        cameraData = mainCamera.backgroundCamera.GetUniversalAdditionalCameraData();
        cameraData.requiresColorTexture = false;
        cameraData.requiresDepthTexture = false;
    }
    
    private static BackgroundAssetReference OverrideBackgroundIfStoryboardHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        var info = handle.Setup.TrackDataSegments[0].trackInfoRef;

        if (!Plugin.EnableStoryboards.Value
            || !info.IsCustomFile
            || !StoryboardManager.Instance.TryGetOrCreateStoryboard(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards"), info.customFile.FileNameNoExtension, out var storyboard))
            return defaultBackground;
        
        if (storyboard.TryGetOutParam("DisableBaseBackground", out bool value) && value)
            return BackgroundSystem.UtilityBackgrounds.lowMotionBackground;

        return null;
    }
    
    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        string customAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(customAssetBundlePath))
            Directory.CreateDirectory(customAssetBundlePath);

        var mainCamera = MainCamera.Instance;
        
        Plugin.Logger.LogMessage(Layers.Background.Index);
        
        StoryboardManager.Create(
            new AssetBundleManager(customAssetBundlePath),
            new SceneManager(),
            new Logger(Plugin.Logger));
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        var data = __instance.playStateFirst.trackData;
        var info = data.TrackInfoRef;
        var storyboardManager = StoryboardManager.Instance;

        if (!Plugin.EnableStoryboards.Value
            || !info.IsCustomFile
            || !StoryboardManager.Instance.TryGetOrCreateStoryboard(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards"), info.customFile.FileNameNoExtension, out var storyboard)) {
            storyboardManager.SetCurrentStoryboard(null, null);
            ResetCameraSettings();
            
            return;
        }

        storyboardManager.SetCurrentStoryboard(storyboard, new StoryboardParams(data));
        ApplyOutParams(storyboard);
        storyboardManager.Play();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        StoryboardManager.Instance.SetCurrentStoryboard(null, null);
        ResetCameraSettings();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) {
        if (Input.GetKeyDown(KeyCode.F1)) {
            StoryboardManager.Instance.RecompileCurrentStoryboard(new StoryboardParams(__instance.playStateFirst.trackData));
            
            if (StoryboardManager.Instance.TryGetCurrentStoryboard(out var storyboard))
                ApplyOutParams(storyboard);
            else
                ResetCameraSettings();
        }

        StoryboardManager.Instance.SetTime(__instance.currentRenderingTrackTime, true);
    }

    [HarmonyPatch(typeof(PlayableTrackDataHandle), "Loading"), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PlayableTrackDataHandle_Loading_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var instructionsList = instructions.ToList();
        var operations = new EnumerableOperation<CodeInstruction>();
        var Patches_OverrideBackgroundIfStoryboardHasOverride = typeof(Patches).GetMethod(nameof(OverrideBackgroundIfStoryboardHasOverride), BindingFlags.NonPublic | BindingFlags.Static);

        var match = PatternMatching.Match(instructionsList, new Func<CodeInstruction, bool>[] {
            instr => instr.opcode == OpCodes.Ldloc_1 // backgroundAssetReference
        }).ElementAt(1)[0];
        
        operations.Insert(match.End, new CodeInstruction[] {
            new (OpCodes.Ldarg_0), // this
            new (OpCodes.Call, Patches_OverrideBackgroundIfStoryboardHasOverride),
            new (OpCodes.Stloc_1), // backgroundAssetReference
            new (OpCodes.Ldloc_1) // backgroundAssetReference
        });

        return operations.Enumerate(instructionsList);
    }
}
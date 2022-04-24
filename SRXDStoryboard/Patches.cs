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

namespace SRXDStoryboard; 

public static class Patches {
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
        
        StoryboardManager.Instance.Initialize(new SceneManager(customAssetBundlePath), new Logger(Plugin.Logger));
        StoryboardManager.Instance.TryAddExtension(string.Empty, new StoryboardExtension());

        var freeRootTransform = new GameObject("Manipulator").transform;
        var staticRootTransform = new GameObject("StaticRoot").transform;
        var cameraTransform = __instance.cameraContainerTransform;
        
        while (cameraTransform.childCount > 0)
            cameraTransform.GetChild(0).SetParent(freeRootTransform, false);

        freeRootTransform.SetParent(__instance.cameraContainerTransform, false);
        staticRootTransform.SetParent(__instance.cameraContainerTransform, false);
        freeRootTransform.localPosition = Vector3.zero;
        staticRootTransform.localPosition = new Vector3(0f, 0.59f, -0.38f);
        staticRootTransform.localRotation = Quaternion.Euler(35f, 0f, 0f);
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
            
            return;
        }

        storyboardManager.SetCurrentStoryboard(storyboard, new StoryboardParams(data));
        storyboardManager.Play();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        StoryboardManager.Instance.SetCurrentStoryboard(null, null);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) {
        if (Input.GetKeyDown(KeyCode.F1))
            StoryboardManager.Instance.RecompileCurrentStoryboard(new StoryboardParams(__instance.playStateFirst.trackData));

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
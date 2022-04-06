using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SMU.Extensions;
using SMU.Utilities;
using StoryboardSystem;
using UnityEngine;

namespace SRXDStoryboard; 

public static class Patches {
    private static BackgroundAssetReference OverrideBackgroundIfStoryboardHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        var info = handle.Setup.TrackDataSegments[0].trackInfoRef;

        if (info.IsCustomFile
            && StoryboardManager.Instance.TryGetOrCreateStoryboard(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards"), info.customFile.FileNameNoExtension, out var storyboard)
            && storyboard.HasData)
            return BackgroundSystem.UtilityBackgrounds.lowMotionBackground;
        
        return defaultBackground;
    }
    
    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        string customAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(customAssetBundlePath))
            Directory.CreateDirectory(customAssetBundlePath);
        
        StoryboardManager.Create(new Logger(Plugin.Logger), new AssetBundleManager(customAssetBundlePath), new PostProcessingManager());
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Prefix(Track __instance) {
        var data = __instance.playStateFirst.trackData;
        var info = data.TrackInfoRef;
        var storyboardManager = StoryboardManager.Instance;
        
        if (!info.IsCustomFile
            || !StoryboardManager.Instance.TryGetOrCreateStoryboard(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards"), info.customFile.FileNameNoExtension, out var storyboard)
            || !storyboard.HasData) {
            storyboardManager.SetCurrentStoryboard(null, null, null);
            
            return;
        }

        var mainCamera = MainCamera.Instance;

        storyboardManager.SetCurrentStoryboard(storyboard, new TimeConversion(data),
            new [] { mainCamera.trackCamera.transform, mainCamera.backgroundCamera.transform });
        storyboardManager.Play();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() => StoryboardManager.Instance.SetCurrentStoryboard(null, null, null);

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) {
        if (Input.GetKeyDown(KeyCode.F1)) {
            var mainCamera = MainCamera.Instance;
            
            StoryboardManager.Instance.RecompileCurrentStoryboard(
                new TimeConversion(__instance.playStateFirst.trackData),
                new [] { mainCamera.trackCamera.transform, mainCamera.backgroundCamera.transform });
        }
        
        StoryboardManager.Instance.SetTime(__instance.currentRenderingTrackTime, true);
    }

    [HarmonyPatch(typeof(TrackData), nameof(TrackData.BackgroundAssetReference), MethodType.Getter), HarmonyPrefix]
    private static bool TrackData_get_BackgroundAssetReference_Prefix(TrackData __instance, ref BackgroundAssetReference __result) {
        if (!__instance.IsCustom
            || !StoryboardManager.Instance.TryGetOrCreateStoryboard(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards"), __instance.CustomFile.FileNameNoExtension, out var storyboard)
            || !storyboard.HasData)
            return true;
        
        __result = BackgroundSystem.UtilityBackgrounds.lowMotionBackground;

        return false;
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
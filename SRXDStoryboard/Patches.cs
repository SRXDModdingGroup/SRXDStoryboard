﻿using System.IO;
using HarmonyLib;
using StoryboardSystem;

namespace SRXDStoryboard; 

public static class Patches {
    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        string customAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(customAssetBundlePath))
            Directory.CreateDirectory(customAssetBundlePath);
        
        StoryboardManager.Create(__instance.cameraContainerTransform, new Logger(Plugin.Logger), new AssetBundleManager(customAssetBundlePath), new PostProcessingManager());
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        var playState = __instance.playStateFirst;
        var info = playState.TrackInfoRef;
        
        if (!info.IsCustomFile) {
            StoryboardManager.Instance.UnloadStoryboard();
            
            return;
        }

        string fileRef = info.customFile.FileNameNoExtension;
        string storyboardPath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Storyboards");
        
        if (!Directory.Exists(storyboardPath))
            Directory.CreateDirectory(storyboardPath);

        string filePath = Path.Combine(storyboardPath, fileRef);
        
        if (!File.Exists(Path.ChangeExtension(filePath, ".txt"))) {
            Plugin.Logger.LogMessage($"Did not find {filePath}");
            StoryboardManager.Instance.UnloadStoryboard();
            
            return;
        }
        
        StoryboardManager.Instance.LoadStoryboard(filePath, new TimeConversion(playState.trackData));
        StoryboardManager.Instance.Play();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() => StoryboardManager.Instance.Stop();

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) => StoryboardManager.Instance.SetTime(__instance.currentRenderingTrackTime, true);
}
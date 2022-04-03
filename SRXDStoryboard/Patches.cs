using System.IO;
using HarmonyLib;
using StoryboardSystem;

namespace SRXDStoryboard; 

public static class Patches {
    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        string customAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(customAssetBundlePath))
            Directory.CreateDirectory(customAssetBundlePath);
        
        StoryboardManager.Create(__instance.cameraContainerTransform, new AssetBundleManager(customAssetBundlePath), new PostProcessingManager(), new Logger(Plugin.Logger));
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

        string filePath = Path.Combine(storyboardPath, Path.ChangeExtension(fileRef, ".sbrd"));
        
        if (!File.Exists(filePath)) {
            StoryboardManager.Instance.UnloadStoryboard();
            
            return;
        }
        
        StoryboardManager.Instance.LoadStoryboard(filePath, new TimeConversion(playState.trackData));
    }
}
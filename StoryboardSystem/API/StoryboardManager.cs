namespace StoryboardSystem; 

public static class StoryboardManager {
    internal static IAssetBundleManager AssetBundleManager { get; private set; }
    
    internal static IPostProcessingManager PostProcessingManager { get; private set; }
}
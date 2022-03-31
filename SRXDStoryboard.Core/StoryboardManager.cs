namespace SRXDStoryboard.Core; 

public static class StoryboardManager {
    public static IAssetBundleManager AssetBundleManager { get; private set; }
    
    public static IPostProcessingManager PostProcessingManager { get; private set; }
}
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class StoryboardData {
    public LoadedExternalObjectReference[] ExternalObjectReferences { get; }
    public LoadedAssetBundleReference[] AssetBundleReferences { get; }
    public LoadedAssetReference[] AssetReferences { get; }
    public LoadedInstanceReference[] InstanceReferences { get; }
    public LoadedPostProcessingReference[] PostProcessingReferences { get; }
    public TimelineBuilder[] TimelineBuilders { get; }
    public Dictionary<string, object> OutParams { get; }
    

    public StoryboardData(
        LoadedExternalObjectReference[] externalObjectReferences,
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingReference[] postProcessingReferences,
        TimelineBuilder[] timelineBuilders,
        Dictionary<string, object> outParams) {
        ExternalObjectReferences = externalObjectReferences;
        AssetBundleReferences = assetBundleReferences;
        AssetReferences = assetReferences;
        InstanceReferences = instanceReferences;
        PostProcessingReferences = postProcessingReferences;
        TimelineBuilders = timelineBuilders;
        OutParams = outParams;
    }
}
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class StoryboardData {
    public LoadedAssetBundleReference[] AssetBundleReferences { get; }
    public LoadedAssetReference[] AssetReferences { get; }
    public LoadedInstanceReference[] InstanceReferences { get; }
    public LoadedPostProcessingMaterialReference[] PostProcessReferences { get; }
    public LoadedExternalObjectReference[] ExternalObjectReferences { get; }
    public CameraIdentifier[] CameraIdentifiers { get; }
    public List<TimelineBuilder> TimelineBuilders { get; }
    public Dictionary<string, object> OutParams { get; }

    public StoryboardData(
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        LoadedExternalObjectReference[] externalObjectReferences,
        CameraIdentifier[] cameraIdentifiers,
        List<TimelineBuilder> timelineBuilders,
        Dictionary<string, object> outParams) {
        AssetBundleReferences = assetBundleReferences;
        AssetReferences = assetReferences;
        InstanceReferences = instanceReferences;
        PostProcessReferences = postProcessReferences;
        ExternalObjectReferences = externalObjectReferences;
        CameraIdentifiers = cameraIdentifiers;
        TimelineBuilders = timelineBuilders;
        OutParams = outParams;
    }
}
namespace SRXDStoryboard.Core; 

public class Storyboard {
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private Event[] events;
    private Curve[] curves;

    public Storyboard(
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        Event[] events, Curve[] curves) {
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.events = events;
        this.curves = curves;
        this.postProcessReferences = postProcessReferences;
    }

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime == toTime)
            return;

        foreach (var @event in events)
            @event.Evaluate(fromTime, toTime);

        foreach (var curve in curves)
            curve.Evaluate(fromTime, toTime);
    }

    public void Load() {
        foreach (var reference in assetBundleReferences)
            reference.Load();
        
        foreach (var reference in assetReferences)
            reference.Load();
        
        foreach (var reference in instanceReferences)
            reference.Load();

        foreach (var reference in postProcessReferences)
            reference.Load();
    }

    public void Unload() {
        foreach (var reference in postProcessReferences)
            reference.Unload();
        
        foreach (var reference in instanceReferences)
            reference.Unload();
        
        foreach (var reference in assetReferences)
            reference.Unload();
        
        foreach (var reference in assetBundleReferences)
            reference.Unload();
    }
}
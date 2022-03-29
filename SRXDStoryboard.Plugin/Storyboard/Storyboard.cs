namespace SRXDStoryboard.Plugin; 

public class Storyboard {
    private string assetBundleName;
    private AssetBundleReference assetBundleReference;
    private ObjectReference[] assetReferences;
    private ObjectReference[] instanceReferences;
    private Event[] events;
    private Curve[] curves;

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime == toTime)
            return;

        foreach (var @event in events) {
            @event.Evaluate(fromTime, toTime);
        }

        foreach (var curve in curves)
            curve.Evaluate(fromTime, toTime);
    }

    public void Load() {
        assetBundleReference.Load();
        
        foreach (var reference in assetReferences)
            reference.Load();
        
        foreach (var reference in instanceReferences)
            reference.Load();
    }

    public void Unload() {
        foreach (var reference in instanceReferences)
            reference.Unload();
        
        foreach (var reference in assetReferences)
            reference.Unload();
        
        assetBundleReference.Unload();
    }
}
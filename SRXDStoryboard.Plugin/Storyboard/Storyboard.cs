﻿namespace SRXDStoryboard.Plugin; 

public class Storyboard {
    private AssetBundleReference[] assetBundleReferences;
    private ObjectReference[] assetReferences;
    private ObjectReference[] instanceReferences;
    private Event[] events;
    private Curve[] curves;

    public Storyboard(AssetBundleReference[] assetBundleReferences, ObjectReference[] assetReferences, ObjectReference[] instanceReferences, Event[] events, Curve[] curves) {
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.events = events;
        this.curves = curves;
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
    }

    public void Unload() {
        foreach (var reference in instanceReferences)
            reference.Unload();
        
        foreach (var reference in assetReferences)
            reference.Unload();
        
        foreach (var reference in assetBundleReferences)
            reference.Unload();
    }
}
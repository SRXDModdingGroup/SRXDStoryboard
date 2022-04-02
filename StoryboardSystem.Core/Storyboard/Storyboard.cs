using System;
using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal class Storyboard {
    private ITimeConversion timeConversion;
    private LoadedAssetBundleReference[] assetBundleReferences;
    private LoadedAssetReference[] assetReferences;
    private LoadedInstanceReference[] instanceReferences;
    private LoadedPostProcessingMaterialReference[] postProcessReferences;
    private Dictionary<Binding, EventBuilder> eventBuilders;
    private Dictionary<Binding, CurveBuilder> curveBuilders;
    private Event[] events;
    private Curve[] curves;

    public Storyboard(
        ITimeConversion timeConversion,
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        Dictionary<Binding, EventBuilder> eventBuilders,
        Dictionary<Binding, CurveBuilder> curveBuilders) {
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.eventBuilders = eventBuilders;
        this.curveBuilders = curveBuilders;
        this.postProcessReferences = postProcessReferences;
    }

    public void Evaluate(float fromTime, float toTime) {
        if (fromTime == toTime)
            return;

        foreach (var @event in events)
            @event.Evaluate(fromTime, toTime);

        foreach (var curve in curves)
            curve.Evaluate(toTime);
    }

    public void Load(Action<string> errorCallback) {
        foreach (var reference in assetBundleReferences)
            reference.Load();
        
        foreach (var reference in assetReferences)
            reference.Load();
        
        foreach (var reference in instanceReferences)
            reference.Load();

        foreach (var reference in postProcessReferences)
            reference.Load();

        var curvesList = new List<Curve>();

        foreach (var pair in curveBuilders) {
            if (Binder.TryCreatePropertyFromBinding(pair.Key, out var property))
                curvesList.Add(property.CreateCurve(pair.Value, timeConversion));
            else
                errorCallback($"Failed to bind property for {pair.Key}");
        }

        var eventsList = new List<Event>();

        foreach (var pair in eventBuilders) {
            if (Binder.TryCreateActionFromBinding(pair.Key, out var action))
                eventsList.AddRange(pair.Value.CreateEvents(action, timeConversion));
            else
                errorCallback($"Failed to bind event for {pair.Key}");
        }

        events = eventsList.ToArray();
        curves = curvesList.ToArray();
    }

    public void Unload() {
        events = null;
        curves = null;
        
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
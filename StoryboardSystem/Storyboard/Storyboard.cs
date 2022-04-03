﻿using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

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
    private float lastTime;
    private bool loaded;

    public Storyboard(
        ITimeConversion timeConversion,
        LoadedAssetBundleReference[] assetBundleReferences,
        LoadedAssetReference[] assetReferences,
        LoadedInstanceReference[] instanceReferences,
        LoadedPostProcessingMaterialReference[] postProcessReferences,
        Dictionary<Binding, EventBuilder> eventBuilders,
        Dictionary<Binding, CurveBuilder> curveBuilders) {
        this.timeConversion = timeConversion;
        this.assetBundleReferences = assetBundleReferences;
        this.assetReferences = assetReferences;
        this.instanceReferences = instanceReferences;
        this.eventBuilders = eventBuilders;
        this.curveBuilders = curveBuilders;
        this.postProcessReferences = postProcessReferences;
    }

    public void Evaluate(float time, bool triggerEvents) {
        if (!loaded || time == lastTime)
            return;

        foreach (var curve in curves)
            curve.Evaluate(time);

        if (triggerEvents) {
            foreach (var @event in events)
                @event.Evaluate(lastTime, time);
        }

        lastTime = time;
    }

    public bool TryLoad(ILogger logger) {
        bool success = true;
        
        foreach (var reference in assetBundleReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in assetReferences)
            success = reference.TryLoad() && success;
        
        foreach (var reference in instanceReferences)
            success = reference.TryLoad() && success;

        foreach (var reference in postProcessReferences)
            success = reference.TryLoad() && success;

        if (!success) {
            Unload();
            
            return false;
        }

        var curvesList = new List<Curve>();

        foreach (var pair in curveBuilders) {
            if (Binder.TryCreateValuePropertyFromBinding(pair.Key, out var property))
                curvesList.Add(property.CreateCurve(pair.Value, timeConversion));
            else {
                logger.LogWarning($"Failed to bind value property for {pair.Key}");
                success = false;
            }
        }

        var eventsList = new List<Event>();

        foreach (var pair in eventBuilders) {
            if (Binder.TryCreateEventPropertyFromBinding(pair.Key, out var property))
                eventsList.AddRange(property.CreateEvents(pair.Value, timeConversion));
            else {
                logger.LogWarning($"Failed to bind event property for {pair.Key}");
                success = false;
            }
        }

        if (!success) {
            Unload();

            return false;
        }

        events = eventsList.ToArray();
        curves = curvesList.ToArray();
        lastTime = -1f;
        loaded = true;

        return true;
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

        loaded = false;
    }

    public void SetPostProcessingEnabled(bool enabled) {
        if (!loaded)
            return;

        foreach (var reference in postProcessReferences)
            reference.SetEnabled(enabled);
    }
}
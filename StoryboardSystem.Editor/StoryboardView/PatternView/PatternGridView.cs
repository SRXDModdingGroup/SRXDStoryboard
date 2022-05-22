using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace StoryboardSystem.Editor; 

public class PatternGridView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler {
    [SerializeField] private Transform laneViewsLayout;
    [SerializeField] private GameObject laneViewPrefab;

    private float scroll;
    private float scale = 8f;
    private InstancePool<LaneView> laneViews;

    public void UpdateView(Pattern pattern) {
        var lanes = pattern.Lanes;
        
        laneViews.SetCount(lanes.Count);

        for (int i = 0; i < lanes.Count; i++) {
            laneViews[i].UpdateView(lanes[i]);
            laneViews[i].UpdateScroll(scroll, scale);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnScroll(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    private void Awake() {
        laneViews = new InstancePool<LaneView>(laneViewsLayout, laneViewPrefab);
    }

    private void SetScroll(float scroll, float scale) {
        this.scroll = scroll;
        this.scale = scale;
        
        foreach (var laneView in laneViews)
            laneView.UpdateScroll(scroll, scale);
    }
}
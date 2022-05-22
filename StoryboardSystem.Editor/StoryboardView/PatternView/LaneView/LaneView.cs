using System;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class LaneView : MonoBehaviour {
    [SerializeField] private RectTransform framesRoot;
    [SerializeField] private GameObject frameViewPrefab;

    private RectTransform rectTransform;
    private InstancePool<FrameView> frameViews;

    public void UpdateView(Lane lane) {
        var frames = lane.Frames;
        
        frameViews.SetCount(frames.Count);

        for (int i = 0; i < frames.Count; i++)
            frameViews[i].UpdateView(frames[i]);
    }

    public void UpdateScroll(float scroll, float scale) {
        float end = scroll + scale;

        foreach (var frameView in frameViews)
            frameView.UpdateScroll(framesRoot, scroll, end);
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        frameViews = new InstancePool<FrameView>(framesRoot, frameViewPrefab);
    }
}
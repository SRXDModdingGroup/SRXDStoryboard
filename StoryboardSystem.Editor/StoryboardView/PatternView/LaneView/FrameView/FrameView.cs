using System;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class FrameView : MonoBehaviour {
    private Frame frame;
    private RectTransform rectTransform;
    
    public void UpdateView(Frame frame) {
        this.frame = frame;
    }

    public void UpdateScroll(RectTransform parent, float start, float end) {
        float time = (float) frame.Time;

        if (frame.Time < start || frame.Time > end) {
            gameObject.SetActive(false);
            
            return;
        }
        
        gameObject.SetActive(true);
        rectTransform.anchoredPosition = new Vector2(parent.sizeDelta.x * Mathf.InverseLerp(start, end, time), 0f);
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }
}
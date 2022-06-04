using System;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public abstract class ViewElement : MonoBehaviour {
    protected ViewInfo ViewInfo { get; private set; }

    public void UpdateView(ViewInfo info) {
        ViewInfo = info;
        UpdateView();
    }
    
    protected abstract void UpdateView();

    private StoryboardView storyboardView;

    private void Awake() {
        var target = transform;

        while (target != null) {
            if (target.TryGetComponent(out storyboardView)) {
                storyboardView.AddElement(this);

                return;
            }
            
            target = target.parent;
        }
    }

    private void OnDestroy() {
        if (storyboardView != null)
            storyboardView.RemoveElement(this);
    }
}
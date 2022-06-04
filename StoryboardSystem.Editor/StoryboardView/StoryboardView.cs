using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardView : MonoBehaviour {
    private HashSet<ViewElement> viewElements;

    public void UpdateView(ViewInfo info) {
        foreach (var viewElement in viewElements)
            viewElement.UpdateView(info);
    }

    public void AddElement(ViewElement element) => viewElements.Add(element);

    public void RemoveElement(ViewElement element) => viewElements.Remove(element);

    private void Awake() {
        viewElements = new HashSet<ViewElement>();
    }
}
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardView : MonoBehaviour {
    private ViewInfo info;
    
    public void UpdateView(ViewInfo info) {
        this.info = info;
    }
}
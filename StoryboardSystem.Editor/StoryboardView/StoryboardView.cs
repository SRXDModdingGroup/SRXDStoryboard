using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardView : MonoBehaviour {
    [SerializeField] private PatternView patternView;
    
    private ViewInfo info;
    
    public void UpdateView(ViewInfo info) {
        this.info = info;
    }
}
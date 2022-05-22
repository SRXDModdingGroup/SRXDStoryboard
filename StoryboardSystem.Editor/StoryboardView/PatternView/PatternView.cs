using UnityEngine;

namespace StoryboardSystem.Editor; 

public class PatternView : MonoBehaviour {
    [SerializeField] private PatternGridView patternGridView;
    
    public void UpdateView(Pattern pattern) {
        patternGridView.UpdateView(pattern);
    }
}
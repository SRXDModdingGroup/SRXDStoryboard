using UnityEngine;

namespace StoryboardSystem.Editor; 

public class PatternView : MonoBehaviour {
    [SerializeField] private PatternGridView patternGridView;
    
    public void UpdateInfo(ProjectSetup setup, Pattern pattern) {
        patternGridView.UpdateInfo(setup, pattern);
    }
}
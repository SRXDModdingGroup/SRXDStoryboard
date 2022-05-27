using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardView : MonoBehaviour {
    [SerializeField] private PatternView patternView;

    private int selectedPatternIndex;
    private StoryboardProject project;
    
    public void UpdateInfo(ViewInfo info) {
        project = info.Project;
        UpdatePatternView();
    }

    private void SetSelectedPatternIndex(int index) {
        selectedPatternIndex = index;
        UpdatePatternView();
    }

    private void UpdatePatternView() {
        patternView.UpdateInfo(project.Setup, project.Patterns[selectedPatternIndex]);
    }
}
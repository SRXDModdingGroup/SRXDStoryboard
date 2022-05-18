using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private StoryboardView view;
    
    private StoryboardProject project;
    private UndoRedo undoRedo;
    
    private void Awake() {
        undoRedo = new UndoRedo();
    }

    private void BeginEdit() {
        undoRedo.BeginNewAction();
    }

    private void EndEdit() {
        undoRedo.CompleteAction();
        UpdateView();
    }

    private void UpdateView() {
        view.UpdateView(new ViewInfo(project, undoRedo.CanRedo(), undoRedo.CanRedo()));
    }

    private void CreateNewProject(ProjectSetup setup) {
        undoRedo.Clear();
        project = new StoryboardProject(setup);
        UpdateView();
    }

    private void CreateNewPattern() {
        string patternName;
        int index = 0;

        while (true) {
            patternName = $"NewPattern_{index}";

            bool exists = false;

            foreach (var pattern in project.Patterns) {
                if (patternName != pattern.Name)
                    continue;

                exists = true;

                break;
            }
            
            if (!exists)
                break;

            index++;
        }

        int patternIndex = project.Patterns.Count;
        
        BeginEdit();
        Do();
        undoRedo.AddSubAction(() => Undo(patternIndex), Do);
        EndEdit();
        
        void Do() => project.Patterns.Add(new Pattern(patternName, Channel.CreateChannelsFromSetup(project.Setup), new List<PatternInstance>()));
        
        void Undo(int patternIndex) => project.Patterns.RemoveAt(patternIndex);
    }
}
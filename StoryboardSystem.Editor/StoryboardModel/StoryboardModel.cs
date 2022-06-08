using System;
using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public partial class StoryboardModel : MonoBehaviour {
    public StoryboardProject Project { get; private set; }

    public event Action Changed;
    
    private UndoRedo undoRedo;

    public void CreateNewProject(ProjectSetup setup) {
        undoRedo.Clear();
        Project = new StoryboardProject(setup);
    }

    public void Undo() {
        if (!undoRedo.CanUndo)
            return;
        
        undoRedo.Undo();
        Changed?.Invoke();
    }

    public void Redo() {
        if (!undoRedo.CanRedo)
            return;
        
        undoRedo.Redo();
        Changed?.Invoke();
    }
    
    public IEditBlock CreateEditBlock() => new EditBlock(Project, undoRedo.CreateAction(), OnEditCompleted);

    private void Awake() => undoRedo = new UndoRedo();

    private void Start() {
        CreateNewProject(new ProjectSetup(new RigSetup[] {
            new("test", "Test", 1, RigType.Event, new RigParameterSetup[] {
                new("param", "Param", RigValueType.Int, Vector3.zero, Vector3.zero, Vector3.one, true, true)
            })
        }, new [] { 0d, 1d, 2d, 3d }));
    }
    
    private void OnEditCompleted() => Changed?.Invoke();

    private string GetUniquePatternName() {
        string patternName;
        int index = 0;

        while (true) {
            patternName = $"NewPattern_{index}";

            bool exists = false;

            foreach (var pattern in Project.Patterns) {
                if (patternName != pattern.Name)
                    continue;

                exists = true;

                break;
            }
            
            if (!exists)
                break;

            index++;
        }

        return patternName;
    }
}
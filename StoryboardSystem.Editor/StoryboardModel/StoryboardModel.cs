using System;
using System.Collections.Generic;
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
        Changed?.Invoke();
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
    
    public IEditBlock CreateEditBlock() => new EditBlock(Project, undoRedo.CreateAction(), Changed);

    private void Awake() => undoRedo = new UndoRedo();

    private void Start() {
        var rigDefinitions = new List<RigDefinitionSetup> {
            new(new List<RigEventSetup> {
                new("event", "Event", RigEventType.Event, new List<RigParameterSetup> {
                    new("param", "Param", RigValueType.Float, Vector3.zero, Vector3.zero, Vector3.one, true, false)
                })
            })
        };
        
        CreateNewProject(new ProjectSetup(rigDefinitions, new List<RigSetup> { new("rig", "Rig", 1, rigDefinitions[0]) }, new [] { 0d, 1d, 2d, 3d }));
    }

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
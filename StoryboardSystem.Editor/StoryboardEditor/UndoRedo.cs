using System;
using System.Collections.Generic;

namespace StoryboardSystem.Editor;

public class UndoRedo {
    private struct UndoRedoPair {
        public Action Undo { get; }
        
        public Action Redo { get; }

        public UndoRedoPair(Action undo, Action redo) {
            Undo = undo;
            Redo = redo;
        }
    }

    private int currentStackIndex = -1;
    private List<List<UndoRedoPair>> stack = new();
    private List<UndoRedoPair> currentAction;

    public void BeginNewAction() => currentAction = new List<UndoRedoPair>();

    public void CompleteAction() {
        currentStackIndex++;
        
        while (stack.Count > currentStackIndex)
            stack.RemoveAt(stack.Count - 1);
        
        stack.Add(currentAction);
    }

    public void AddSubAction(Action undo, Action redo) => currentAction.Add(new UndoRedoPair(undo, redo));

    public void Clear() {
        currentStackIndex = -1;
        stack.Clear();
    }

    public void Undo() {
        if (currentStackIndex < 0)
            return;

        var action = stack[currentStackIndex];

        for (int i = action.Count - 1; i >= 0; i--)
            action[i].Undo();

        currentStackIndex--;
    }

    public void Redo() {
        if (currentStackIndex >= stack.Count - 1)
            return;

        currentStackIndex++;
        
        var action = stack[currentStackIndex];

        for (int i = 0; i < action.Count; i++)
            action[i].Redo();
    }

    public bool CanUndo() => currentStackIndex >= 0;

    public bool CanRedo() => currentStackIndex < stack.Count - 1;
}
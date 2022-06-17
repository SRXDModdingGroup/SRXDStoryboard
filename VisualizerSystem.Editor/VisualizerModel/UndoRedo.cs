using System;
using System.Collections.Generic;

namespace VisualizerSystem.Editor;

public partial class UndoRedo {
    public bool CanUndo => currentStackIndex >= 0;

    public bool CanRedo => currentStackIndex < stack.Count - 1;

    private int currentStackIndex = -1;
    private List<UndoRedoAction> stack = new();
    private bool actionOngoing;

    public void Clear() {
        if (actionOngoing)
            throw new InvalidOperationException("The current action must be completed first");
        
        currentStackIndex = -1;
        stack.Clear();
    }

    public void Undo() {
        if (actionOngoing)
            throw new InvalidOperationException("The current action must be completed first");
        
        if (currentStackIndex < 0)
            return;

        stack[currentStackIndex].Undo();
        currentStackIndex--;
    }

    public void Redo() {
        if (actionOngoing)
            throw new InvalidOperationException("The current action must be completed first");
        
        if (currentStackIndex >= stack.Count - 1)
            return;

        currentStackIndex++;
        stack[currentStackIndex].Redo();
    }
    
    public IUndoRedoAction CreateAction() {
        actionOngoing = true;
        
        return new UndoRedoAction(OnActionCompleted);
    }

    private void OnActionCompleted(UndoRedoAction action) {
        currentStackIndex++;
        
        while (stack.Count > currentStackIndex)
            stack.RemoveAt(stack.Count - 1);
        
        stack.Add(action);
        actionOngoing = false;
    }
}
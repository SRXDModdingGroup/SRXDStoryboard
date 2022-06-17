using System;
using System.Collections.Generic;

namespace VisualizerSystem.Editor;

public partial class UndoRedo {
    private class UndoRedoAction : IUndoRedoAction {
        private struct UndoRedoPair {
            public Action Undo { get; }
        
            public Action Redo { get; }

            public UndoRedoPair(Action undo, Action redo) {
                Undo = undo;
                Redo = redo;
            }
        }
        
        private List<UndoRedoPair> subActions;
        private Action<UndoRedoAction> completed;
        private bool disposed;

        public UndoRedoAction(Action<UndoRedoAction> completed) {
            this.completed = completed;
            subActions = new List<UndoRedoPair>();
        }
        
        public void AddSubAction(Action undo, Action redo) => subActions.Add(new UndoRedoPair(undo, redo));

        public void Undo() {
            for (int i = subActions.Count - 1; i >= 0; i--)
                subActions[i].Undo();
        }

        public void Redo() {
            for (int i = 0; i < subActions.Count; i++)
                subActions[i].Redo();
        }

        public void Dispose() {
            if (disposed)
                return;
            
            completed.Invoke(this);
            disposed = true;
        }
    }
}
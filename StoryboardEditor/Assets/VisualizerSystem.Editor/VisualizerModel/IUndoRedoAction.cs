using System;

namespace VisualizerSystem.Editor {
    public interface IUndoRedoAction : IDisposable {
        public void AddSubAction(Action undo, Action redo);
    }
}
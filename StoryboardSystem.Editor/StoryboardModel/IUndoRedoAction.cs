using System;

namespace StoryboardSystem.Editor;

public interface IUndoRedoAction : IDisposable {
    public void AddSubAction(Action undo, Action redo);
}
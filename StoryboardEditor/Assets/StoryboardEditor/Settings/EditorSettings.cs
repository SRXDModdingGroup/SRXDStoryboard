using System.Collections.Generic;

public class EditorSettings {
    public Dictionary<BindableAction, Binding> Bindings { get; } = new() {
        { BindableAction.Undo, new Binding("Undo", "z", InputModifier.Control) },
        { BindableAction.Redo, new Binding("Redo", "y", InputModifier.Control) },
        { BindableAction.Copy, new Binding("Copy", "c", InputModifier.Control) },
        { BindableAction.Paste, new Binding("Paste", "v", InputModifier.Control) },
        { BindableAction.PasteAndInsert, new Binding("Paste and Insert", "v", InputModifier.Control | InputModifier.Shift) },
        { BindableAction.Duplicate, new Binding("Duplicate", "d", InputModifier.Control) },
        { BindableAction.DuplicateAndInsert, new Binding("Duplicate and Insert", "d", InputModifier.Control | InputModifier.Shift) },
        { BindableAction.Rename, new Binding("Rename", "r", InputModifier.Control) }
    };
}
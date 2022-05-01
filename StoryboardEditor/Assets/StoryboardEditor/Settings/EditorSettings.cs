using System.Collections.Generic;

public class EditorSettings {
    public Dictionary<BindableAction, Binding> Bindings { get; } = new() {
        { BindableAction.Undo, new Binding("Undo", "z", InputModifier.Control) },
        { BindableAction.Redo, new Binding("Redo", "y", InputModifier.Control) }
    };
}
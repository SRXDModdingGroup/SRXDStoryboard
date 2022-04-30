public class EditorSettings {
    public Binding[] Bindings { get; } = {
        new("Undo", BindableAction.Undo, "z", InputModifier.Control),
        new("Redo", BindableAction.Redo, "y", InputModifier.Control)
    };
}
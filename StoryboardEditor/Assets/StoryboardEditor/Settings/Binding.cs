using UnityEngine;

public readonly struct Binding {
    public string Name { get; }
        
    public BindableAction ActionId { get; }
        
    public string InputString { get; }
        
    public InputModifier Modifiers { get; }

    public Binding(string name, BindableAction actionId, string inputString, InputModifier modifiers) {
        Name = name;
        ActionId = actionId;
        InputString = inputString;
        Modifiers = modifiers;
    }
}
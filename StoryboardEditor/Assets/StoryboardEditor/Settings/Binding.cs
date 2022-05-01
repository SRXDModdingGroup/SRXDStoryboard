public readonly struct Binding {
    public string Name { get; }
        
    public string InputString { get; }
        
    public InputModifier Modifiers { get; }

    public Binding(string name, string inputString, InputModifier modifiers) {
        Name = name;
        InputString = inputString;
        Modifiers = modifiers;
    }
}
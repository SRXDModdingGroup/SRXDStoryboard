using System;

[Flags]
public enum InputModifier {
    None = 0,
    Shift = 1 << 0,
    Control = 1 << 1,
    Alt = 1 << 2
}

public static class InputModifierExtensions {
    public static bool HasAnyModifiers(this InputModifier input) => input != 0;
    public static bool HasAnyModifiers(this InputModifier input, InputModifier desired) => (input & desired) != 0;
    
    public static bool HasAllModifiers(this InputModifier input, InputModifier desired) => (input & desired) == desired;

    public static bool HasExactModifiers(this InputModifier input, InputModifier desired) => input == desired;
}
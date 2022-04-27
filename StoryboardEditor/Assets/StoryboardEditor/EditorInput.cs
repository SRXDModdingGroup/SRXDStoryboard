using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInput : MonoBehaviour {
    [Flags]
    public enum InputModifier {
        None = 0,
        Shift = 1 << 0,
        Control = 1 << 1,
        Alt = 1 << 2
    }
    
    public event Action<InputModifier> Backspace;
    
    public event Action<InputModifier> Tab;
    
    public event Action<InputModifier> Return;
    
    public event Action<InputModifier> Escape;
    
    public event Action<InputModifier> Space;
    
    public event Action<InputModifier> Delete;
    
    public event Action<Vector2Int, InputModifier> Direction;

    public event Action<string, InputModifier> Character;

    // ReSharper disable Unity.PerformanceAnalysis
    private void Update() {
        if (!Input.anyKeyDown)
            return;
        
        var modifiers = GetModifiers();

        if (Input.GetKeyDown(KeyCode.Backspace))
            Backspace?.Invoke(modifiers);
        
        if (Input.GetKeyDown(KeyCode.Tab))
            Tab?.Invoke(modifiers);
        
        if (Input.GetKeyDown(KeyCode.Return))
            Return?.Invoke(modifiers);
        
        if (Input.GetKeyDown(KeyCode.Escape))
            Escape?.Invoke(modifiers);
        
        if (Input.GetKeyDown(KeyCode.Space))
            Space?.Invoke(modifiers);
        
        if (Input.GetKeyDown(KeyCode.Delete))
            Delete?.Invoke(modifiers);

        var direction = Vector2Int.zero;
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
            direction += Vector2Int.left;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            direction += Vector2Int.right;
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
            direction += Vector2Int.up;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            direction += Vector2Int.down;
        
        if (direction != Vector2Int.zero)
            Direction?.Invoke(direction, modifiers);

        string inputString = Input.inputString;
        
        if (!string.IsNullOrEmpty(inputString) && inputString.Length == 1 && !char.IsControl(inputString, 0))
            Character?.Invoke(inputString, modifiers);
    }

    public static InputModifier GetModifiers() {
        var modifiers = InputModifier.None;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            modifiers |= InputModifier.Shift;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            modifiers |= InputModifier.Control;

        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            modifiers |= InputModifier.Alt;
        
        return modifiers;
    }
}

public static class EditorInputExtensions {
    public static bool HasModifiers(this EditorInput.InputModifier input, EditorInput.InputModifier desired) => (input & desired) == desired;
}

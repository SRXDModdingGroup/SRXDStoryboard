using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorInput {
    public event Action<InputModifier> Backspace;
    
    public event Action<InputModifier> Tab;
    
    public event Action<InputModifier> Return;
    
    public event Action<InputModifier> Escape;
    
    public event Action<InputModifier> Space;
    
    public event Action<InputModifier> Delete;
    
    public event Action<Vector2Int, InputModifier> Direction;

    public event Action<string, InputModifier> Character;

    private EditorSettings settings;
    private Dictionary<BindableAction, List<Action>> bindingsDict { get; }

    public EditorInput(EditorSettings settings) {
        this.settings = settings;
        
        bindingsDict = new Dictionary<BindableAction, List<Action>>();

        foreach (var pair in settings.Bindings)
            bindingsDict.Add(pair.Key, new List<Action>());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateInput() {
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
        
        if (!IsSingleCharacter(inputString))
            return;

        if (modifiers.HasAnyModifiers(InputModifier.Control | InputModifier.Alt)) {
            inputString = inputString.ToLowerInvariant();
            
            foreach (var (bindableAction, binding) in settings.Bindings) {
                if (binding.InputString != inputString || !modifiers.HasExactModifiers(binding.Modifiers) || !bindingsDict.TryGetValue(bindableAction, out var actions))
                    continue;
                
                foreach (var action in actions)
                    action?.Invoke();
            }
        }
        else
            Character?.Invoke(inputString, modifiers);
    }
    
    public void Bind(BindableAction actionId, Action action) {
        if (bindingsDict.TryGetValue(actionId, out var actions))
            actions.Add(action);
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

    private static bool IsSingleCharacter(string inputString) => !string.IsNullOrEmpty(inputString) && inputString.Length == 1 && !char.IsControl(inputString, 0);
}

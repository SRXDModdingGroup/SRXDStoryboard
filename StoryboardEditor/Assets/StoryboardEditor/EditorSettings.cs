using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorSettings {
    public enum BindableAction {
        
    }
    
    private readonly struct Binding {
        public string Name { get; }
        
        public BindableAction Action { get; }
        
        public KeyCode KeyCode { get; }
        
        public int Modifiers { get; }

        public Binding(string name, BindableAction action, KeyCode keyCode, int modifiers) {
            Name = name;
            Action = action;
            KeyCode = keyCode;
            Modifiers = modifiers;
        }
    }

    private const int SHIFT = 1 << 0;
    private const int CRTL = 1 << 1;
    private const int ALT = 1 << 2;

    private Binding[] Bindings = {
        
    };

    private Dictionary<BindableAction, Action> bindingsDict = new() {
        
    };
}

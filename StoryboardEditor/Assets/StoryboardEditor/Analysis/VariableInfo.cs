﻿using System.Collections.Generic;
using UnityEngine;

public class VariableInfo {
    public string Name { get; }
    
    public Vector2Int Declaration { get; }

    public List<Vector2Int> Usages { get; } = new();

    public VariableInfo(string name, Vector2Int declaration) {
        Name = name;
        Declaration = declaration;
    }
}
﻿using System.Collections.Generic;
using UnityEngine;

public class VariableInfo {
    public string Name { get; }
    
    public Vector2Int Declaration { get; set; }

    public List<VariableUsage> Usages { get; }
    
    public ProcedureInfo ProcedureInfo { get; set; }

    public VariableInfo(string name, Vector2Int declaration) {
        Name = name;
        Declaration = declaration;
        Usages = new List<VariableUsage>();
    }
}
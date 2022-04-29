using System.Collections.Generic;

public readonly struct ProcedureInfo {
    public int Index { get; }
        
    public string Name { get; }
        
    public List<string> ArgNames { get; }

    public ProcedureInfo(int index, string name, List<string> argNames) {
        Index = index;
        Name = name;
        ArgNames = argNames;
    }
}
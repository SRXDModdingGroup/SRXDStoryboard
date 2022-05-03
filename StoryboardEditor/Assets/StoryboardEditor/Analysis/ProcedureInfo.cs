using System.Collections.Generic;

public class ProcedureInfo {
    public int Index { get; }
        
    public string Name { get; }
    
    public List<string> ArgNames { get; }
    
    public Dictionary<string, VariableInfo> Locals { get; }

    public ProcedureInfo(int index, string name, List<string> argNames, Dictionary<string, VariableInfo> locals) {
        Index = index;
        Name = name;
        ArgNames = argNames;
        Locals = locals;
    }
}
using System.Collections.Generic;

public class ProcedureInfo {
    public int Row { get; }
        
    public string Name { get; }
    
    public List<string> ArgNames { get; }
    
    public Dictionary<string, VariableInfo> Locals { get; }
    
    public VariableInfo VariableInfo { get; }

    public ProcedureInfo(int row, string name, List<string> argNames, Dictionary<string, VariableInfo> locals, VariableInfo variableInfo) {
        Row = row;
        Name = name;
        ArgNames = argNames;
        Locals = locals;
        VariableInfo = variableInfo;
    }
}
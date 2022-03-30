using System.Collections.Generic;

namespace SRXDStoryboard.Plugin; 

public class Scope {
    public Scope Parent { get; }


    public Timestamp StartTime { get; }

    public int ReturnIndex { get; }

    private Scope global;
    private Dictionary<string, object> variables;

    public Scope(Scope parent, Scope global, Timestamp startTime, int returnIndex, Dictionary<string, object> variables) {
        Parent = parent;
        this.global = global;
        StartTime = startTime;
        ReturnIndex = returnIndex;
        this.variables = variables;
    }

    public void SetValue(string name, object value) => variables[name] = value;
    
    public bool TryGetValue(string name, out object value)
        => variables.TryGetValue(name, out value) || global != null && global.variables.TryGetValue(name, out value);
}
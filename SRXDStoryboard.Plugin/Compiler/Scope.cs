using System.Collections.Generic;

namespace SRXDStoryboard.Plugin; 

public class Scope {
    public Scope Parent { get; }

    public Timestamp StartTime { get; }

    public int ReturnIndex { get; }

    private Dictionary<string, object> globals;
    private Dictionary<string, object> locals;

    public Scope(Scope parent, Timestamp startTime, int returnIndex, Dictionary<string, object> globals, Dictionary<string, object> locals) {
        Parent = parent;
        StartTime = startTime;
        ReturnIndex = returnIndex;
        this.globals = globals;
        this.locals = locals;
    }

    public void SetValue(string name, object value) => locals[name] = value;
    
    public bool TryGetValue(string name, out object value)
        => locals.TryGetValue(name, out value) || globals.TryGetValue(name, out value);
}
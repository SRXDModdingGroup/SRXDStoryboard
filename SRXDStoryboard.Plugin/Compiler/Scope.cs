using System.Collections.Generic;

namespace SRXDStoryboard.Plugin; 

public class Scope {
    public Scope Parent { get; }

    public Timestamp StartTime { get; }
    
    public int StartIndex { get; }
    
    public int ReturnIndex { get; }
    
    public int Iterations { get; }
    
    public int CurrentIteration { get; private set; }

    private Dictionary<string, object> globals;
    private Dictionary<string, object> locals;

    public Scope(Scope parent, Timestamp startTime, int startIndex, int returnIndex, int iterations, Dictionary<string, object> globals, Dictionary<string, object> locals) {
        Parent = parent;
        StartTime = startTime;
        StartIndex = startIndex;
        ReturnIndex = returnIndex;
        Iterations = iterations;
        CurrentIteration = 0;
        this.globals = globals;
        this.locals = locals;
    }

    public void SetValue(string name, object value) => locals[name] = value;
    
    public bool TryGetValue(string name, out object value)
        => locals.TryGetValue(name, out value) || globals.TryGetValue(name, out value);

    public bool CheckForRecursion(int index) => StartIndex != index && (Parent == null || Parent.CheckForRecursion(index));

    public bool NextIteration() {
        CurrentIteration++;

        return CurrentIteration < Iterations;
    }
}
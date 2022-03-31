using System.Collections.Generic;

namespace StoryboardSystem.Core; 

internal class Scope {
    public Scope Parent { get; }

    public int StartIndex { get; }
    
    public int ReturnIndex { get; }

    public int CurrentIteration { get; private set; }

    private int iterations;
    private Timestamp startTime;
    private Timestamp every;
    private Dictionary<Name, object> globals;
    private Dictionary<Name, object> locals;

    public Scope(Scope parent, int startIndex, int returnIndex, int iterations, Timestamp startTime, Timestamp every, Dictionary<Name, object> globals, Dictionary<Name, object> locals) {
        Parent = parent;
        StartIndex = startIndex;
        ReturnIndex = returnIndex;
        CurrentIteration = 0;
        this.iterations = iterations;
        this.startTime = startTime;
        this.every = every;
        this.globals = globals;
        this.locals = locals;
    }

    public void SetValue(Name name, object value) => locals[name] = value;
    
    public bool TryGetValue(Name name, out object value)
        => locals != null && locals.TryGetValue(name, out value) || globals.TryGetValue(name, out value);

    public bool CheckForRecursion(int index) => StartIndex != index && (Parent == null || Parent.CheckForRecursion(index));

    public bool NextIteration() {
        CurrentIteration++;

        return CurrentIteration < iterations;
    }

    public Timestamp GetGlobalTime(Timestamp localTime) => localTime + startTime + CurrentIteration * every;
}
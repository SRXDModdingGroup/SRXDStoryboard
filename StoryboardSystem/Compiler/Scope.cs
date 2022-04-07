using System.Collections.Generic;

namespace StoryboardSystem; 

internal class Scope {
    private static readonly Name ITER_NAME = new("iter");
    private static readonly Name COUNT_NAME = new("count");
    
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
        locals[ITER_NAME] = 0;
        locals[COUNT_NAME] = iterations;
    }

    public void SetValue(Name name, object value) => locals[name] = value;
    
    public bool TryGetValue(Name name, out object value)
        => locals != null && locals.TryGetValue(name, out value) || globals.TryGetValue(name, out value);

    public bool CheckForRecursion(int index) => StartIndex != index && (Parent == null || Parent.CheckForRecursion(index));

    public bool NextIteration() {
        CurrentIteration++;
        locals[ITER_NAME] = CurrentIteration;

        return CurrentIteration < iterations;
    }

    public Timestamp GetGlobalTime(Timestamp localTime) => localTime + startTime + CurrentIteration * every;
}
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class Procedure {
    private static readonly Name ITER_NAME = new("iter");
    private static readonly Name COUNT_NAME = new("count");
    
    public int StartIndex { get; }
    
    public int ReturnIndex { get; private set; }

    public int CurrentIteration { get; private set; }
    
    public Procedure Parent { get; private set; }
    
    public Name[] ArgNames { get; }

    private int iterations;
    private Timestamp startTime;
    private Timestamp every;
    private Dictionary<Name, object> globals;
    private Dictionary<Name, object> locals;

    public Procedure(int startIndex, Dictionary<Name, object> globals, Dictionary<Name, object> locals, Name[] argNames) {
        StartIndex = startIndex;
        this.globals = globals;
        this.locals = locals;
        ArgNames = argNames;
        CurrentIteration = -1;
    }

    public void Init(int returnIndex, Procedure parent, int iterations, Timestamp startTime, Timestamp every) {
        ReturnIndex = returnIndex;
        Parent = parent;
        CurrentIteration = 0;
        this.iterations = iterations;
        this.startTime = startTime;
        this.every = every;
        
        if (locals == null)
            return;
        
        locals.Clear();
        locals[ITER_NAME] = 0;
        locals[COUNT_NAME] = iterations;
    }

    public void SetValue(Name name, object value) => locals[name] = value;
    
    public bool TryGetValue(Name name, out object value)
        => locals != null && locals.TryGetValue(name, out value) || globals.TryGetValue(name, out value);

    public bool CheckForRecursion() => CurrentIteration < 0;

    public bool NextIteration() {
        CurrentIteration++;
        locals[ITER_NAME] = CurrentIteration;

        if (CurrentIteration < iterations)
            return true;

        CurrentIteration = -1;

        return false;
    }

    public Timestamp GetGlobalTime(Timestamp localTime) => localTime + startTime + CurrentIteration * every;
}
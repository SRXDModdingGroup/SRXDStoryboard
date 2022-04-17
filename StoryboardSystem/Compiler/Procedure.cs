using System.Collections.Generic;

namespace StoryboardSystem; 

internal class Procedure {
    private static readonly Name ITER_NAME = new("iter");
    private static readonly Name COUNT_NAME = new("count");

    public string Name { get; }
    
    public int StartIndex { get; }
    
    public int ReturnIndex { get; private set; }
    
    public Procedure Parent { get; private set; }
    
    public Name[] ArgNames { get; }

    private int iterations;
    private int currentIteration;
    private Timestamp startTime;
    private Timestamp every;
    private Dictionary<Name, object> globals;
    private Dictionary<Name, object> locals;

    public Procedure(string name, int startIndex, Dictionary<Name, object> globals, Dictionary<Name, object> locals, Name[] argNames) {
        Name = name;
        StartIndex = startIndex;
        this.globals = globals;
        this.locals = locals;
        ArgNames = argNames;
        currentIteration = -1;
    }

    public void Init(int returnIndex, Procedure parent, int iterations, Timestamp startTime, Timestamp every) {
        ReturnIndex = returnIndex;
        Parent = parent;
        currentIteration = 0;
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

    public bool CheckForRecursion() => currentIteration < 0;

    public bool NextIteration() {
        currentIteration++;
        locals[ITER_NAME] = currentIteration;

        if (currentIteration < iterations)
            return true;

        currentIteration = -1;

        return false;
    }

    public Timestamp GetGlobalTime(Timestamp localTime) => localTime + startTime + currentIteration * every;
}
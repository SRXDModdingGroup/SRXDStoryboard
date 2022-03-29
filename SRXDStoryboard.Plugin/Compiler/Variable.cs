using System.Collections.Generic;

namespace SRXDStoryboard.Plugin; 

public class Variable {
    public virtual object Value { get; }

    public Variable(object value) => Value = value;
    
    private Dictionary<string, Variable> cachedSubVariables;

    public bool TryGetSubVariable(string name, out Variable variable) {
        if (cachedSubVariables != null && cachedSubVariables.TryGetValue(name, out variable))
            return true;

        if (!TryCreateSubVariable(name, out variable))
            return false;

        cachedSubVariables ??= new Dictionary<string, Variable>();
        cachedSubVariables.Add(name, variable);

        return true;
    }

    protected virtual bool TryCreateSubVariable(string name, out Variable variable) {
        variable = null;

        return false;
    }
}
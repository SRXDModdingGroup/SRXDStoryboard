using System.Collections.Generic;

namespace SRXDStoryboard.Plugin; 

public class Variable {
    public virtual object Value { get; }

    public Variable(object value) => Value = value;
    
    private Dictionary<string, object> cachedSubVariables;

    public bool TryGetSubVariable(string name, out object variable) {
        if (cachedSubVariables != null && cachedSubVariables.TryGetValue(name, out variable))
            return true;

        if (!TryCreateSubVariable(name, out variable))
            return false;

        cachedSubVariables ??= new Dictionary<string, object>();
        cachedSubVariables.Add(name, variable);

        return true;
    }

    protected virtual bool TryCreateSubVariable(string name, out object variable) {
        variable = null;

        return false;
    }
}
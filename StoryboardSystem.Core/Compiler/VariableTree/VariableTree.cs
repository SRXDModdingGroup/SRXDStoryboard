using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

internal abstract class VariableTree {
    public abstract object Value { get; }

    private Dictionary<Name, object> cachedSubVariables;

    public bool TryGetSubVariable(Name name, out object variable) {
        if (cachedSubVariables != null && cachedSubVariables.TryGetValue(name, out variable))
            return true;

        if (!TryCreateSubVariable(name, out variable))
            return false;

        cachedSubVariables ??= new Dictionary<Name, object>();
        cachedSubVariables.Add(name, variable);

        return true;
    }

    protected abstract bool TryCreateSubVariable(Name name, out object variable);

    public static object Create(LoadedInstanceReference reference) => reference switch {
        LoadedInstanceReference<Material> material => material,
        LoadedInstanceReference<GameObject> prefab => prefab,
        _ => reference
    };
}
using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

internal class IdentifierTree {
    private object key;
    private IdentifierTree parent;
    private Dictionary<object, IdentifierTree> children;
    private Identifier identifier;

    public IdentifierTree(string name, int referenceIndex) {
        key = referenceIndex;
        parent = null;
        identifier = new Identifier(name, referenceIndex, Array.Empty<object>());
        children = new Dictionary<object, IdentifierTree>();
    }

    private IdentifierTree(object key, IdentifierTree parent) {
        this.key = key;
        this.parent = parent;
        children = new Dictionary<object, IdentifierTree>();
    }

    public IdentifierTree GetChild(object childKey) {
        if (children.TryGetValue(childKey, out var child))
            return child;

        child = new IdentifierTree(childKey, this);
        children.Add(childKey, child);

        return child;
    }

    public Identifier GetIdentifier() {
        if (identifier != null)
            return identifier;

        var parentIdentifier = parent.GetIdentifier();
        object[] parentSequence = parentIdentifier.Sequence;
        object[] newSequence = new object[parentSequence.Length + 1];
        
        Array.Copy(parentSequence, newSequence, parentSequence.Length);
        newSequence[newSequence.Length - 1] = key;

        string name = key switch {
            string str => $"{parentIdentifier}.{str}",
            int intVal => $"{parentIdentifier}[{intVal}]",
            null => $"{parentIdentifier}.NULL",
            var obj => $"{parentIdentifier}.{obj}"
        };

        identifier = new Identifier(name, parentIdentifier.ReferenceIndex, newSequence);

        return identifier;
    }
}
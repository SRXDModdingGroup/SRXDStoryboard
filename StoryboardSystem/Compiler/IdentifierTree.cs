using System.Collections.Generic;

namespace StoryboardSystem; 

internal class IdentifierTree {
    private object key;
    private IdentifierTree parent;
    private Dictionary<object, IdentifierTree> children;
    private Identifier identifier;

    public IdentifierTree(int referenceIndex) {
        key = referenceIndex;
        parent = null;
    }

    private IdentifierTree(object key, IdentifierTree parent) {
        this.key = key;
        this.parent = parent;
    }

    public IdentifierTree GetChild(object childKey) {
        children ??= new Dictionary<object, IdentifierTree>();

        if (children.TryGetValue(childKey, out var child))
            return child;

        child = new IdentifierTree(childKey, this);
        children.Add(childKey, child);

        return child;
    }

    public Identifier GetIdentifier() {
        if (identifier != null)
            return identifier;
        
        using var reverseSequence = PooledList.Get();
        var node = this;

        while (node != null) {
            reverseSequence.Add(node.key);
            node = node.parent;
        }

        if (reverseSequence[reverseSequence.Count - 1] is not int referenceIndex)
            return null;

        object[] sequence = new object[reverseSequence.Count - 1];

        for (int i = 0, j = reverseSequence.Count - 2; i < sequence.Length; i++, j--)
            sequence[i] = reverseSequence[j];

        identifier = new Identifier(referenceIndex, sequence);

        return new Identifier(referenceIndex, sequence);
    }
}
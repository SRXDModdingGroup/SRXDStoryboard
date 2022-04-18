using System;
using System.Collections.Generic;

namespace StoryboardSystem;

public abstract class Controller {
    internal Controller() { }

    internal abstract bool TryCreateBinding(List<Property> properties, out Binding binding);
}

public abstract class Controller<T> : Controller {
    public abstract void Evaluate(float time, Action<T> set);

    internal override bool TryCreateBinding(List<Property> properties, out Binding binding) {
        var propertiesT = new Property<T>[properties.Count];

        for (int i = 0; i < properties.Count; i++) {
            var property = properties[i];

            if (property is not Property<T> propertyT) {
                binding = null;
                
                return false;
            }

            propertiesT[i] = propertyT;
        }

        binding = new Binding<T>(propertiesT, this);

        return true;
    }
}
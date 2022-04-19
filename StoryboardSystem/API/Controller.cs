using System;
using System.Collections.Generic;

namespace StoryboardSystem;

public abstract class Controller {
    protected internal abstract bool IsEvent { get; }
    
    internal Controller() { }

    internal abstract bool TryCreateBinding(List<Property> properties, out Binding binding);
}

public abstract class Controller<T> : Controller {
    protected internal abstract void Evaluate(float time, Action<T> set);

    internal override bool TryCreateBinding(List<Property> properties, out Binding binding) {
        var propertiesT = new Property<T>[properties.Count];
        var type = properties[0].GetType();

        for (int i = 0; i < properties.Count; i++) {
            var property = properties[i];

            if (property.GetType() != type || property is not Property<T> propertyT) {
                binding = null;
                StoryboardManager.Instance.Logger.LogMessage($"Can not bind properties of different types to controller {GetType()}<{typeof(T)}>");
                
                return false;
            }
            
            if (propertyT.IsEvent && !IsEvent)
                StoryboardManager.Instance.Logger.LogMessage($"Binding event property to non-event controller {GetType()}<{typeof(T)}>. This may produce unexpected results");
            else if (!propertyT.IsEvent && IsEvent)
                StoryboardManager.Instance.Logger.LogMessage($"Binding non-event property to event controller {GetType()}<{typeof(T)}>. This may produce unexpected results");

            propertiesT[i] = propertyT;
        }

        binding = new Binding<T>(propertiesT, this);

        return true;
    }
}
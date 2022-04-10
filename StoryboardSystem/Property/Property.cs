using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

internal abstract class Property {
    public abstract bool TryCreateBinding(Property[] properties, List<KeyframeBuilder> keyframeBuilders, IStoryboardParams sParams, out Binding binding);
}

internal abstract class Property<T> : Property {
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);
    
    public override bool TryCreateBinding(Property[] properties, List<KeyframeBuilder> keyframeBuilders, IStoryboardParams sParams, out Binding binding) {
        var propertiesT = new Property<T>[properties.Length];
        var type = GetType();

        for (int i = 0; i < properties.Length; i++) {
            if (properties[i] is not Property<T> propertyT || propertyT.GetType() != type) {
                binding = null;

                return false;
            }

            propertiesT[i] = propertyT;
        }

        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframeBuilders.Count; i++) {
            if (keyframeBuilders[i].TryCreateKeyframe(this, sParams, out keyframes[i]))
                continue;
            
            binding = null;

            return false;
        }

        Array.Sort(keyframes);
        binding = CreateBinding(propertiesT, keyframes);

        return true;
    }

    protected abstract Binding CreateBinding(Property<T>[] properties, Keyframe<T>[] keyframes);
}
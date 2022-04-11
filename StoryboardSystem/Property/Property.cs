using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

internal abstract class Property {
    public abstract bool TryCreateBinding(Property[] properties, List<KeyframeBuilder> keyframeBuilders, IStoryboardParams sParams, ILogger logger, out Binding binding);
}

internal abstract class Property<T> : Property {
    private static readonly float NEAR_KEY_THRESHOLD = 0.001f;
    
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);
    
    public override bool TryCreateBinding(Property[] properties, List<KeyframeBuilder> keyframeBuilders, IStoryboardParams sParams, ILogger logger, out Binding binding) {
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

        if (keyframes.Length > 1) {
            Array.Sort(keyframes);

            int runStartIndex = 0;
            var sortingTimes = new List<float>();

            for (int i = 0; i < keyframes.Length; i++) {
                float time = keyframes[i].Time;
            
                if (i < keyframes.Length - 1 && keyframes[i + 1].Time - time < NEAR_KEY_THRESHOLD)
                    continue;
                
                if (i > runStartIndex) {
                    sortingTimes.Clear();
                    
                    for (int j = 0, k = runStartIndex; j <= i - runStartIndex; j++, k++)
                        sortingTimes.Add(keyframes[k].Time);
            
                    bool sorted;
            
                    do {
                        sorted = true;
            
                        for (int j = 0, k = runStartIndex; j < sortingTimes.Count - 1; j++, k++) {
                            var first = keyframes[k];
                            var second = keyframes[k + 1];
            
                            if (first.Order <= second.Order
                                || Mathf.Abs(first.Time - sortingTimes[k + 1]) >= NEAR_KEY_THRESHOLD
                                || Mathf.Abs(second.Time - sortingTimes[k]) >= NEAR_KEY_THRESHOLD)
                                continue;
                            
                            keyframes[k] = second;
                            keyframes[k + 1] = first;
                            sorted = false;
                        }
                    } while (!sorted);
            
                    for (int j = 0, k = runStartIndex; j < sortingTimes.Count; j++, k++) {
                        var keyframe = keyframes[k];
            
                        keyframes[k] = new Keyframe<T>(sortingTimes[j], keyframe.Value, keyframe.InterpType, 0);
                    }
                }
            
                runStartIndex = i + 1;
            }
        }
        
        binding = CreateBinding(propertiesT, keyframes);

        return true;
    }

    protected abstract Binding CreateBinding(Property<T>[] properties, Keyframe<T>[] keyframes);
}
using System;
using UnityEngine;

namespace StoryboardSystem.Core; 

internal abstract class Binder {
    public static bool TryCreatePropertyFromBinding(Binding binding, out Property property) {
        if (TryResolveBinding(binding, out object result) && result is Property newProperty) {
            property = newProperty;

            return true;
        }

        property = null;

        return false;
    }

    public static bool TryCreateActionFromBinding(Binding binding, out Action action) {
        if (TryResolveBinding(binding, out object result) && result is Action newAction) {
            action = newAction;

            return true;
        }

        action = null;

        return false;
    }

    private static bool TryResolveBinding(Binding binding, out object result) {
        result = binding.Reference.LoadedObject;

        foreach (object item in binding.Sequence) {
            switch (item) {
                case int index when result is object[] arr && index >= 0 && index < arr.Length: {
                    result = arr[index];

                    continue;
                }
                case string name when TryGetSubObject(result, name, out object temp): {
                    result = temp;

                    continue;
                }
            }
            
            result = null;

            return false;
        }

        return true;
    }

    private static bool TryGetSubObject(object parent, string name, out object subObject) {
        switch (parent) {
            case GameObject gameObject:
                
                
                return true;
        }
        
        subObject = null;

        return false;
    }
}
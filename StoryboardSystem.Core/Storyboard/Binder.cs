using System;
using UnityEngine;

namespace StoryboardSystem.Core; 

internal abstract class Binder {
    public static bool TryCreateValuePropertyFromBinding(Binding binding, out ValueProperty property) {
        if (TryResolveBinding(binding, out object result) && result is ValueProperty newProperty) {
            property = newProperty;

            return true;
        }

        property = null;

        return false;
    }

    public static bool TryCreateEventPropertyFromBinding(Binding binding, out EventProperty property) {
        if (TryResolveBinding(binding, out object result) && result is EventProperty newProperty) {
            property = newProperty;

            return true;
        }

        property = null;

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
                var transform = gameObject.transform;

                subObject = name switch {
                    "pos" => new PositionProperty(transform),
                    "rot" => new RotationProperty(transform),
                    "scale" => new ScaleProperty(transform),
                    _ => null
                };

                return subObject != null;
        }
        
        subObject = null;

        return false;
    }
}
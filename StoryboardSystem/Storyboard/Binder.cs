using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

internal abstract class Binder {
    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

    public static bool TryResolveIdentifier(Identifier identifier, List<LoadedObjectReference> objectReferences, out object result) {
        int referenceIndex = identifier.ReferenceIndex;
        
        if (referenceIndex < 0 || referenceIndex >= objectReferences.Count) {
            StoryboardManager.Instance.Logger.LogWarning($"Could not resolve identifier {identifier}: Reference index is not valid");
            result = null;

            return false;
        }
        
        result = objectReferences[referenceIndex].LoadedObject;

        foreach (object item in identifier.Sequence) {
            switch (item) {
                case int index when result is object[] arr && index >= 0 && index < arr.Length:
                    result = arr[index];

                    continue;
                case int index when result is GameObject gameObject && TryGetChildGameObject(gameObject, index, out result):
                    continue;
                case string name when TryGetSubObject(result, name, out object temp):
                    result = temp;

                    continue;
                default:
                    StoryboardManager.Instance.Logger.LogWarning($"Could not resolve identifier {identifier}: Sequence contained an invalid value");
                    result = null;

                    return false;
            }
        }

        if (result != null)
            return true;
        
        StoryboardManager.Instance.Logger.LogWarning($"Could not resolve identifier {identifier}");

        return false;
    }
    
    public static bool TryCreateBinding(KeyValuePair<Identifier, List<Identifier>> identifiers, List<LoadedObjectReference> objectReferences, out Binding binding) {
        binding = null;
        
        var properties = new List<Property>();
        
        if (!TryResolveIdentifier(identifiers.Key, objectReferences, out object obj))
            return false;

        if (obj is not Controller controller) {
            StoryboardManager.Instance.Logger.LogWarning($"{identifiers.Key} is not a controller");
            
            return false;
        }

        foreach (var identifier in identifiers.Value) {
            if (!TryResolveIdentifier(identifier, objectReferences, out obj))
                return false;

            if (obj is not Property property) {
                StoryboardManager.Instance.Logger.LogWarning($"{identifier} is not a property");
                
                return false;
            }

            properties.Add(property);
        }

        if (controller.TryCreateBinding(properties, out binding))
            return true;
        
        StoryboardManager.Instance.Logger.LogWarning($"Could not create binding for {identifiers.Key}");

        return false;
    }

    private static bool TryGetSubObject(object parent, string name, out object subObject) {
        switch (parent) {
            case GameObject gameObject:
                return TryGetGameObjectProperty(gameObject, out subObject);
            case Material material:
                return TryGetMaterialProperty(material, out subObject);
            case PostProcessingInstance postProcess:
                if (name != "enabled")
                    return TryGetMaterialProperty(postProcess.Material, out subObject);
                
                subObject = new PostProcessingEnabledProperty(postProcess);

                return true;
            case Camera camera when name == "fov":
                subObject = new CameraFovProperty(camera);
                
                return true;
            case IStoryboardObject sObject when sObject.TryGetSubObject(name, out subObject):
                return true;
            case Component component:
                return TryGetGameObjectProperty(component.gameObject, out subObject);
            default:
                subObject = null;

                return false;
        }

        bool TryGetGameObjectProperty(GameObject gameObject, out object property) {
            property = name switch {
                "pos" => new PositionProperty(gameObject.transform),
                "rot" => new RotationProperty(gameObject.transform),
                "scale" => new ScaleProperty(gameObject.transform),
                "mat" => gameObject.GetComponent<Renderer>()?.material,
                "mats" => gameObject.GetComponent<Renderer>()?.materials,
                _ => gameObject.transform.Find(name)?.gameObject
            };

            if (property != null)
                return true;
            
            foreach (var sObject in gameObject.GetComponents<IStoryboardObject>()) {
                if (sObject.TryGetSubObject(name, out property))
                    return true;
            }

            property = null;

            return false;
        }

        bool TryGetMaterialProperty(Material material, out object property) {
            if (name == "color" && material.HasColor(COLOR_ID)) {
                property = new MaterialColorProperty(material, COLOR_ID);

                return true;
            }

            if (name[0] != '_')
                name = name.Insert(0, "_");
                
            int id = Shader.PropertyToID(name);

            if (material.HasFloat(id))
                property = new MaterialFloatProperty(material, id);
            else if (material.HasVector(id))
                property = new MaterialVectorProperty(material, id);
            else if (material.HasColor(id))
                property = new MaterialColorProperty(material, id);
            else {
                property = null;

                return false;
            }

            return true;
        }
    }

    private static bool TryGetChildGameObject(GameObject gameObject, int index, out object childObject) {
        var transform = gameObject.transform;

        if (transform.childCount == 0) {
            childObject = null;
                    
            return false;
        }

        childObject = transform.GetChild(MathUtility.Mod(index, transform.childCount)).gameObject;
                    
        return true;
    }
}
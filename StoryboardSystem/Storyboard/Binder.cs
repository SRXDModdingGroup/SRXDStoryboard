using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem; 

internal abstract class Binder {
    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

    public static bool TryResolveIdentifier(Identifier identifier, List<LoadedObjectReference> objectReferences, ILogger logger, out object result) {
        int referenceIndex = identifier.ReferenceIndex;
        
        if (referenceIndex < 0 || referenceIndex >= objectReferences.Count) {
            logger.LogWarning($"Could not resolve identifier {identifier}: Reference index is not valid");
            result = null;

            return false;
        }
        
        result = objectReferences[referenceIndex].LoadedObject;

        foreach (object item in identifier.Sequence) {
            switch (item) {
                case int index when result is object[] arr: {
                    if (index < 0 || index >= arr.Length)
                        break;

                    result = arr[index];

                    continue;
                }
                case int index when result is GameObject gameObject: {
                    if (TryGetChildGameObject(gameObject, index, out result))
                        continue;
                    
                    break;
                }
                case string name when TryGetSubObject(result, name, out object temp): {
                    result = temp;

                    continue;
                }
            }
            
            logger.LogWarning($"Could not resolve identifier {identifier}: Sequence contained an invalid value");
            result = null;

            return false;
        }

        if (result != null)
            return true;
        
        logger.LogWarning($"Could not resolve identifier {identifier}");

        return false;
    }

    public static bool TryBindProperty(Identifier identifier, List<LoadedObjectReference> objectReferences, ILogger logger, out Property property) {
        if (TryResolveIdentifier(identifier, objectReferences, logger, out object result) && result is Property newProperty) {
            property = newProperty;

            return true;
        }

        property = null;

        return false;
    }

    private static bool TryGetSubObject(object parent, string name, out object subObject) {
        switch (parent) {
            case GameObject gameObject:
                var transform = gameObject.transform;

                subObject = name switch {
                    "pos" => new PositionProperty(transform),
                    "rot" => new RotationProperty(transform),
                    "scale" => new ScaleProperty(transform),
                    "mat" => gameObject.GetComponent<Renderer>()?.material,
                    "mats" => gameObject.GetComponent<Renderer>()?.materials,
                    _ => transform.Find(name)?.gameObject
                };

                return subObject != null;
            case Material material:
                return TryGetMaterialProperty(material, out subObject);
            case PostProcessingInstance postProcess:
                if (name != "enabled")
                    return TryGetMaterialProperty(postProcess.Material, out subObject);
                
                subObject = new PostProcessingEnabledProperty(postProcess);

                return true;
        }
        
        subObject = null;

        return false;

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
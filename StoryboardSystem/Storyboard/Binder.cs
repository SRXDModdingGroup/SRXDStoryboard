using UnityEngine;

namespace StoryboardSystem; 

internal abstract class Binder {
    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");
    
    public static bool TryBindProperty(Identifier identifier, out Property property) {
        if (TryResolveIdentifier(identifier, out object result) && result is Property newProperty) {
            property = newProperty;

            return true;
        }

        property = null;

        return false;
    }

    private static bool TryResolveIdentifier(Identifier identifier, out object result) {
        result = identifier.Reference.LoadedObject;

        foreach (object item in identifier.Sequence) {
            switch (item) {
                case int index when result is object[] arr: {
                    if (index < 0 || index >= arr.Length)
                        break;

                    result = arr[index];

                    continue;
                }
                case int index when result is GameObject gameObject: {
                    var transform = gameObject.transform;

                    if (index < 0 || index >= transform.childCount)
                        break;

                    result = transform.GetChild(index).gameObject;
                    
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
                    "mat" => gameObject.GetComponent<Renderer>()?.material,
                    "mats" => gameObject.GetComponent<Renderer>()?.materials,
                    _ => transform.Find(name)
                };

                return subObject != null;
            case Material material:
                if (name == "color") {
                    subObject = new MaterialColorProperty(material, COLOR_ID);

                    return true;
                }

                if (name[0] != '_')
                    name = name.Insert(0, "_");
                
                int id = Shader.PropertyToID(name);

                if (material.HasFloat(id))
                    subObject = new MaterialFloatProperty(material, id);
                else if (material.HasVector(id))
                    subObject = new MaterialVectorProperty(material, id);
                else if (material.HasColor(id))
                    subObject = new MaterialColorProperty(material, id);
                else
                    break;

                return true;
        }
        
        subObject = null;

        return false;
    }
}
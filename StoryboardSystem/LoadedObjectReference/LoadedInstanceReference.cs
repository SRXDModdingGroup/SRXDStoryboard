using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal class LoadedInstanceReference : LoadedObjectReference {
    public override object LoadedObject => instance;

    private Identifier template;
    private Identifier parent;
    private int layer;
    private string layerS;
    private Object instance;

    public LoadedInstanceReference(Identifier template, Identifier parent, int layer, string layerS) {
        this.template = template;
        this.parent = parent;
        this.layer = layer;
        this.layerS = layerS;
    }

    public override void Unload(ISceneManager sceneManager) {
        if (instance != null)
            Object.Destroy(instance);
        
        instance = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, Dictionary<Identifier, List<Identifier>> bindings, ISceneManager sceneManager, IStoryboardParams sParams) {
        if (!Binder.TryResolveIdentifier(template, objectReferences, out object obj))
            return false;
        
        if (obj is not Object uObj) {
            StoryboardManager.Instance.Logger.LogWarning($"{template} is not a Unity object");

            return false;
        }
        
        instance = Object.Instantiate(uObj);
        
        if (instance is GameObject gameObject) {
            Transform parentTransform;

            if (parent == null)
                parentTransform = null;
            else if (Binder.TryResolveIdentifier(parent, objectReferences, out object parentObject)) {
                switch (parentObject) {
                    case Transform newParentTransform:
                        parentTransform = newParentTransform;
                        break;
                    case GameObject parentGameObject:
                        parentTransform = parentGameObject.transform;
                        break;
                    case Component component:
                        parentTransform = component.transform;
                        break;
                    default:
                        StoryboardManager.Instance.Logger.LogWarning($"{parent} is not a gameObject, transform, or component");

                        return false;
                }
            }
            else
                return false;

            gameObject.name = template.ToString();
        
            var transform = gameObject.transform;

            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            int newLayer;

            if (string.IsNullOrWhiteSpace(layerS))
                newLayer = layer;
            else
                newLayer = LayerMask.NameToLayer(layerS);
            
            SetLayer(transform);
            
            void SetLayer(Transform subTransform) {
                subTransform.gameObject.layer = newLayer;

                for (int i = 0; i < subTransform.childCount; i++)
                    SetLayer(subTransform.GetChild(i));
            }
        }

        sceneManager.InitializeObject(instance);

        return true;
    }

    public override bool TrySerialize(BinaryWriter writer) {
        writer.Write((byte) ObjectReferenceType.Instance);
        template.Serialize(writer);
        
        if (parent == null)
            writer.Write(false);
        else {
            writer.Write(true);
            parent.Serialize(writer);
        }

        if (string.IsNullOrWhiteSpace(layerS)) {
            writer.Write(false);
            writer.Write(layer);
        }
        else {
            writer.Write(true);
            writer.Write(layerS);
        }

        return true;
    }

    public static LoadedInstanceReference Deserialize(BinaryReader reader) {
        var template = Identifier.Deserialize(reader);
        Identifier parent;

        if (reader.ReadBoolean())
            parent = Identifier.Deserialize(reader);
        else
            parent = null;
        
        int layer;
        string layerS;

        if (reader.ReadBoolean()) {
            layer = 0;
            layerS = reader.ReadString();
        }
        else {
            layer = reader.ReadInt32();
            layerS = string.Empty;
        }

        return new LoadedInstanceReference(template, parent, layer, layerS);
    }
}
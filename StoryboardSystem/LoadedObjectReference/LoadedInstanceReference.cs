using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal abstract class LoadedInstanceReference : LoadedObjectReference {
    public abstract void Unload();

    public abstract bool TryLoad(ISceneManager sceneManager, ILogger logger);
}

internal class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public override object LoadedObject => instance;
    
    private string name;
    private object parent;
    private int layer;
    private T instance;
    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template, string name, object parent, int layer) {
        this.template = template;
        this.name = name;
        this.parent = parent;
        this.layer = layer;
    }

    public override void Unload() {
        if (instance == null)
            return;
        
        Object.Destroy(instance);
        instance = null;
    }

    public override bool TryLoad(ISceneManager sceneManager, ILogger logger) {
        if (template.Asset == null) {
            logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }

        object resolvedObject;
        
        switch (parent) {
            case Identifier identifier when Binder.TryResolveIdentifier(identifier, out resolvedObject):
                break;
            case LoadedExternalObjectReference reference: {
                if (reference.LoadedObject == null) {
                    logger.LogWarning($"Target parent is null");

                    return false;
                }

                resolvedObject = reference.LoadedObject;
                
                break;
            }
            case null:
                resolvedObject = null;
                
                break;
            default:
                return false;
        }

        Transform parentTransform;

        if (resolvedObject == null)
            parentTransform = null;
        else {
            switch (resolvedObject) {
                case Transform newParentTransform:
                    parentTransform = newParentTransform;
                    break;
                case GameObject parentGameObject:
                    parentTransform = parentGameObject.transform;
                    break;
                default:
                    logger.LogWarning($"Target parent is not a gameObject or transform");

                    return false;
            }
        }
        
        instance = Object.Instantiate(template.Asset);
        
        if (instance is GameObject gameObject) {
            gameObject.name = name;
        
            var transform = gameObject.transform;

            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            SetLayer(transform);
            
            void SetLayer(Transform subTransform) {
                subTransform.gameObject.layer = layer;

                for (int i = 0; i < subTransform.childCount; i++)
                    SetLayer(subTransform.GetChild(i));
            }
        }

        sceneManager.InitializeObject(instance);

        return true;
    }
}
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
    private int sceneRootIndex;
    private int layer;
    private T instance;
    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template, string name, int sceneRootIndex, int layer) {
        this.template = template;
        this.name = name;
        this.sceneRootIndex = sceneRootIndex;
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
        
        instance = Object.Instantiate(template.Asset);
        
        if (instance is GameObject gameObject) {
            gameObject.name = name;
        
            var transform = gameObject.transform;

            if (sceneRootIndex >= 0 && sceneRootIndex < sceneManager.SceneRoots.Count)
                transform.SetParent(sceneManager.SceneRoots[sceneRootIndex]);
            else
                transform.SetParent(null, false);

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
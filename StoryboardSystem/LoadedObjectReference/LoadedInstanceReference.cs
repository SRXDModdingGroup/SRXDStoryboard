using UnityEngine;

namespace StoryboardSystem;

internal abstract class LoadedInstanceReference : LoadedObjectReference {
    public abstract bool TryLoad(Transform[] sceneRoots);
}

internal class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public override object LoadedObject => Instance;

    protected T Instance { get; private set; }
    
    protected int Layer { get; }

    private string name;
    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template, string name, int layer) {
        this.template = template;
        this.name = name;
        Layer = layer;
    }

    public override bool TryLoad() {
        if (template.Asset == null) {
            StoryboardManager.Instance.Logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }
        
        Instance = Object.Instantiate(template.Asset);

        return Instance is not GameObject;
    }

    public override bool TryLoad(Transform[] sceneRoots) {
        if (template.Asset == null) {
            StoryboardManager.Instance.Logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }
        
        Instance = Object.Instantiate(template.Asset);

        if (Instance is not GameObject gameObject)
            return true;

        if (Layer < 0 || Layer >= sceneRoots.Length)
            return false;

        gameObject.name = name;
        
        var transform = gameObject.transform;
            
        transform.SetParent(sceneRoots[Layer], false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        return true;
    }

    public override void Unload() {
        if (Instance == null)
            return;
        
        Object.Destroy(Instance);
        Instance = null;
    }
}
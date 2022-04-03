using UnityEngine;

namespace StoryboardSystem; 

internal abstract class LoadedInstanceReference : LoadedObjectReference { }

internal class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public override object LoadedObject => Instance;
    
    public T Instance { get; private set; }

    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template) => this.template = template;

    public override bool TryLoad() {
        if (template.Asset == null) {
            StoryboardManager.Instance.Logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }
        
        Instance = Object.Instantiate(template.Asset);

        if (Instance is not GameObject gameObject)
            return true;
        
        var transform = gameObject.transform;
            
        transform.SetParent(StoryboardManager.Instance.SceneRoot, false);
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
using UnityEngine;

namespace StoryboardSystem; 

internal abstract class LoadedInstanceReference : LoadedObjectReference { }

internal class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public override object LoadedObject => Instance;
    
    public T Instance { get; private set; }

    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template) => this.template = template;

    public override void Load() {
        Instance = Object.Instantiate(template.Asset);

        if (Instance is not GameObject gameObject)
            return;
        
        var transform = gameObject.transform;
            
        transform.SetParent(StoryboardManager.Instance.SceneRoot, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public override void Unload() {
        Object.Destroy(Instance);
        Instance = null;
    }
}
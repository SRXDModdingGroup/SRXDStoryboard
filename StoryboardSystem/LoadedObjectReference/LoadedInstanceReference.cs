using UnityEngine;

namespace StoryboardSystem; 

internal abstract class LoadedInstanceReference : LoadedObjectReference { }

internal class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public override object LoadedObject => instance;

    private LoadedAssetReference<T> template;
    private T instance;

    public LoadedInstanceReference(LoadedAssetReference<T> template) => this.template = template;

    public override void Load() {
        instance = Object.Instantiate(template.Asset);

        if (instance is not GameObject gameObject)
            return;
        
        var transform = gameObject.transform;
            
        transform.SetParent(StoryboardManager.Instance.SceneRoot, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public override void Unload() {
        Object.Destroy(instance);
        instance = null;
    }
}
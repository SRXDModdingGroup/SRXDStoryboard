using UnityEngine;

namespace StoryboardSystem.Core; 

public abstract class LoadedInstanceReference : LoadedObjectReference { }

public class LoadedInstanceReference<T> : LoadedInstanceReference where T : Object {
    public T Instance { get; private set; }
    
    private LoadedAssetReference<T> template;

    public LoadedInstanceReference(LoadedAssetReference<T> template) => this.template = template;

    public override void Load() => Instance = Object.Instantiate(template.Asset);

    public override void Unload() {
        Object.Destroy(Instance);
        Instance = null;
    }
}
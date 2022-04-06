using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal abstract class LoadedInstanceReference : LoadedObjectReference { }

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

    public override void Unload() {
        if (Instance == null)
            return;
        
        Object.Destroy(Instance);
        Instance = null;
    }

    public override bool TryLoad() {
        if (template.Asset == null) {
            StoryboardManager.Instance.Logger.LogWarning($"Failed to create instance of {template.AssetName}");
            
            return false;
        }

        var sceneManager = StoryboardManager.Instance.SceneManager;
        
        Instance = Object.Instantiate(template.Asset);
        
        if (Instance is GameObject gameObject) {
            gameObject.name = name;
        
            var transform = gameObject.transform;
            
            transform.SetParent(sceneManager.GetLayerRoot(Layer), false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        sceneManager.InitializeObject(Instance, Layer);

        return true;
    }
}
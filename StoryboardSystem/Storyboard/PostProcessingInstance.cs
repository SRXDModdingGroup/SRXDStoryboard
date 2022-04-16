using UnityEngine;

namespace StoryboardSystem; 

internal class PostProcessingInstance {
    public Material Material { get; }
    
    private ISceneManager sceneManager;
    private Camera camera;

    public PostProcessingInstance(ISceneManager sceneManager, Material material, Camera camera) {
        Material = material;
        this.sceneManager = sceneManager;
        this.camera = camera;
    }

    public void Add() => sceneManager.AddPostProcessingInstance(Material, camera);

    public void Remove() {
        sceneManager.RemovePostProcessingInstance(Material, camera);
        Object.Destroy(Material);
    }

    public void SetEnabled(bool enabled) => sceneManager.SetPostProcessingInstanceEnabled(Material, camera, enabled);
}
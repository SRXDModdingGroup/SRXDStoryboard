using UnityEngine;

namespace StoryboardSystem.Core; 

public readonly struct PostProcessingInfo {
    public Material Material { get; }
    
    public int Layer { get; }

    public PostProcessingInfo(Material material, int layer) {
        Material = material;
        Layer = layer;
    }
}
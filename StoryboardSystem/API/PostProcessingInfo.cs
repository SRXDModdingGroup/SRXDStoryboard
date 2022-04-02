using UnityEngine;

namespace StoryboardSystem; 

public readonly struct PostProcessingInfo {
    public Material Material { get; }
    
    public int Layer { get; }

    public PostProcessingInfo(Material material, int layer) {
        Material = material;
        Layer = layer;
    }
}
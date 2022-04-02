using UnityEngine;

namespace StoryboardSystem.Core; 

internal abstract class MaterialProperty<T> : ValueProperty<T> {
    protected Material Material { get; }
    
    protected int Id { get; }

    protected MaterialProperty(Material material, int id) {
        Material = material;
        Id = id;
    }
}
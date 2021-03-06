using UnityEngine;

namespace StoryboardSystem; 

internal abstract class MaterialProperty<T> : VectorProperty<T> {
    protected Material Material { get; }
    
    protected int Id { get; }

    protected MaterialProperty(Material material, int id) {
        Material = material;
        Id = id;
    }
}
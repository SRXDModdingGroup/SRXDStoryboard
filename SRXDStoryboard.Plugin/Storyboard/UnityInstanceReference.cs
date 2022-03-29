using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class UnityInstanceReference<T> : UnityObjectReference<T> where T : Object {
    private T template;

    public UnityInstanceReference(T template) => this.template = template;

    public override void Load() => Value = Object.Instantiate(template);

    public override void Unload() {
        Object.Destroy(Value);
        Value = null;
    }
}
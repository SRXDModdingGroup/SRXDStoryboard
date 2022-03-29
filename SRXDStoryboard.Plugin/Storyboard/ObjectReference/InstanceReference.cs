using UnityEngine;

namespace SRXDStoryboard.Plugin; 

public class InstanceReference<T> : ObjectReference<T> where T : Object {
    private AssetReference<T> template;

    public InstanceReference(AssetReference<T> template) => this.template = template;

    public override void Load() => Value = Object.Instantiate(template.Value);

    public override void Unload() {
        Object.Destroy(Value);
        Value = null;
    }
}
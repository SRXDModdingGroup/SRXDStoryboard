using UnityEngine;

namespace VisualizerSystem.Rigging {
    public abstract class RigTarget : MonoBehaviour {
        [SerializeField] private string rigKey;
        [SerializeField] private int rigIndex;

        public void Bind(VisualizerScene scene) {
            if (scene.TryGetRig(rigKey, rigIndex, out var rig))
                Bind(rig);
        }
    
        protected abstract void Bind(VisualizerRig rig);
    }
}
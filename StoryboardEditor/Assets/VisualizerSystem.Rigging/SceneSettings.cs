using UnityEngine;

namespace VisualizerSystem.Rigging {
    public class SceneSettings : ScriptableObject {
        [SerializeField] private GameObject scenePrefab;
        [SerializeField] private RigSettings[] rigs;
        [SerializeField] private string[] dependencies;

        public GameObject ScenePrefab => scenePrefab;
    
        public RigSettings[] Rigs => rigs;

        public string[] Dependencies => dependencies;
    }
}
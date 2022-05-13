using UnityEngine;

namespace StoryboardSystem.Rigging; 

public class ArrayRig : MonoBehaviour {
    [SerializeField] private string name;
    [SerializeField] private string elementRigName;
    [SerializeField] private GameObject[] elements;

    private StoryboardRig[] rigs;

    private void Awake() {
        rigs = new StoryboardRig[elements.Length];

        for (int i = 0; i < elements.Length; i++) {
            var element = elements[i];

            foreach (var rig in element.GetComponents<StoryboardRig>()) {
                if (rig.name != elementRigName)
                    continue;

                rigs[i] = rig;

                break;
            }
        }
    }
}
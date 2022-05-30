using UnityEngine;

namespace StoryboardSystem.Editor; 

public class FrameBlock : MonoBehaviour {
    public GridElement GridElement { get; private set; }

    private void Awake() {
        GridElement = GetComponent<GridElement>();
    }
}
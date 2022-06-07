using UnityEngine;

namespace StoryboardSystem.Editor; 

public class PatternInstanceBlock : MonoBehaviour {
    public GridElement GridElement { get; private set; }

    private void Awake() => GridElement = GetComponent<GridElement>();
}
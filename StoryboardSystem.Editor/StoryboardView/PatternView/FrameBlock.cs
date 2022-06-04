using UnityEngine;

namespace StoryboardSystem.Editor; 

public class FrameBlock : MonoBehaviour {
    [SerializeField] private GrabHandle moveHandle;

    public GrabHandle MoveHandle => moveHandle;
    
    public GridElement GridElement { get; private set; }

    private void Awake() => GridElement = GetComponent<GridElement>();
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class GridElement : MonoBehaviour {
    [SerializeField] private GameObject visuals;
    [SerializeField] private RectTransform[] handles;
    
    public int Lane {
        get => lane;
        set {
            lane = value;
            needsUpdate = true;
        }
    }

    public float Position {
        get => position;
        set {
            position = value;
            needsUpdate = true;
        }
    }

    public float Size {
        get => size;
        set {
            size = value;
            needsUpdate = true;
        }
    }

    public IReadOnlyList<RectTransform> Handles => handles;

    private bool needsUpdate;
    private int lane;
    private float position;
    private float size;
    private Grid grid;
    private RectTransform rectTransform;

    public void UpdateView() => needsUpdate = true;

    private void Awake() {
        grid = transform.parent.GetComponent<Grid>();
        grid.AddElement(this);
        rectTransform = GetComponent<RectTransform>();
        UpdateState();
    }

    private void LateUpdate() {
        if (needsUpdate)
            UpdateState();
    }

    private void OnDestroy() => grid.RemoveElement(this);

    private void UpdateState() {
        needsUpdate = false;
        
        if (grid.IsInVisibleBounds(position, position + size)) {
            visuals.SetActive(true);
            rectTransform.offsetMin = grid.GridToLocalSpace(Position, Lane);
            rectTransform.offsetMax = grid.GridToLocalSpace(Position + Size, Lane + 1);
        }
        else
            visuals.SetActive(false);
    }
}
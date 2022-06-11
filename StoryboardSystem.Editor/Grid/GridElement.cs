using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class GridElement : View {
    [SerializeField] private GameObject visuals;
    [SerializeField] private RectTransform[] handles;
    
    public int Lane {
        get => lane;
        set {
            lane = value;
            UpdateView();
        }
    }

    public float Position {
        get => position;
        set {
            position = value;
            UpdateView();
        }
    }

    public float Size {
        get => size;
        set {
            size = value;
            UpdateView();
        }
    }

    public IReadOnlyList<RectTransform> Handles => handles;

    private int lane;
    private float position;
    private float size;
    private Grid grid;
    private RectTransform rectTransform;
    
    protected override void DoUpdateView() {
        if (grid.IsInVisibleBounds(position, position + size)) {
            visuals.SetActive(true);
            rectTransform.offsetMin = grid.GridToLocalSpace(Position, Lane);
            rectTransform.offsetMax = grid.GridToLocalSpace(Position + Size, Lane + 1);
        }
        else
            visuals.SetActive(false);
    }

    private void Awake() {
        grid = transform.parent.GetComponent<Grid>();
        grid.AddElement(this);
        rectTransform = GetComponent<RectTransform>();
        UpdateView();
    }

    private void OnDestroy() => grid.RemoveElement(this);
}
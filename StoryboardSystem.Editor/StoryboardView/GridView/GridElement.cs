using System;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class GridElement : MonoBehaviour {
    [SerializeField] private GameObject visuals;
    
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
    
    private bool needsUpdate;
    private int lane;
    private float position;
    private float size;
    private GridView gridView;
    private RectTransform rectTransform;

    private void Awake() {
        gridView = transform.parent.GetComponent<GridView>();
        rectTransform = GetComponent<RectTransform>();
        gridView.ViewChanged += () => needsUpdate = true;
        UpdateState();
    }

    private void LateUpdate() {
        if (needsUpdate)
            UpdateState();
    }

    private void UpdateState() {
        needsUpdate = false;
        
        if (Position <= 1f / gridView.Scale && Position + Size >= 0f) {
            visuals.SetActive(true);
            rectTransform.offsetMin = gridView.GridToLocalSpace(Position, Lane);
            rectTransform.offsetMax = gridView.GridToLocalSpace(Position + Size, Lane + 1);
        }
        else
            visuals.SetActive(false);
    }
}
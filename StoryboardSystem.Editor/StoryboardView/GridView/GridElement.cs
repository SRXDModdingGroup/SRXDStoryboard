using UnityEngine;

namespace StoryboardSystem.Editor; 

public class GridElement : MonoBehaviour {
    public int Lane {
        get => lane;
        set {
            lane = value;
            
            if (GridView != null)
                GridView.ScheduleUpdate();
        }
    }

    public float Position {
        get => position;
        set {
            position = value;
            
            if (GridView != null)
                GridView.ScheduleUpdate();
        }
    }

    public float Size {
        get => size;
        set {
            size = value;
            
            if (GridView != null)
                GridView.ScheduleUpdate();
        }
    }
    
    public GridView GridView { get; private set; }

    private int lane;
    private float position;
    private float size;

    private void Awake() {
        if (transform.parent.TryGetComponent(out GridView gridView))
            GridView = gridView;
    }
}
using UnityEngine;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private GridView gridView;

    private bool rowSelecting;

    private void Awake() {
        gridView.OnDragBegin += OnGridDragBegin;
        gridView.OnDragUpdate += OnGridDragUpdate;
    }

    private void OnGridDragBegin(int row, int column) {
        rowSelecting = column < 0;
        
        if (Input.GetKey(KeyCode.LeftControl))
            gridView.ApplyBoxSelection();

        if (Input.GetKey(KeyCode.LeftShift)) {
            if (rowSelecting)
                gridView.SetRowSelectionEnd(row);
            else
                gridView.SetBoxSelectionEnd(row, column);
        }
        else {
            gridView.ClearBoxSelection();
            
            if (!Input.GetKey(KeyCode.LeftControl))
                gridView.ClearSelection();
            
            if (rowSelecting)
                gridView.SetRowSelectionStart(row);
            else
                gridView.SetBoxSelectionStart(row, column);
        }
    }
    
    private void OnGridDragUpdate(int row, int column) {
        if (rowSelecting)
            gridView.SetRowSelectionEnd(row);
        else
            gridView.SetBoxSelectionEnd(row, column);
    }
    
    private void OnGridDragEnd(int row, int column) {
        
    }
}

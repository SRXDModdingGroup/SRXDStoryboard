using System;
using UnityEngine;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private GridView gridView;

    private bool rowSelecting;

    private void Awake() {
        gridView.OnDragBegin += OnGridDragBegin;
        gridView.OnDragUpdate += OnGridDragUpdate;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            gridView.ClearSelection();
            gridView.ClearBoxSelection();
        }

        bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow);
        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow);

        if (!leftPressed && !rightPressed && !upPressed && !downPressed|| !gridView.TryGetBoxSelectionEnd(out var end))
            return;

        int rowChange = 0;

        if (upPressed)
            rowChange--;

        if (downPressed)
            rowChange++;

        int columnChange = 0;

        if (leftPressed)
            columnChange--;

        if (rightPressed)
            columnChange++;
            
        if (Input.GetKey(KeyCode.LeftControl))
            gridView.ApplyBoxSelection();
        
        if (Input.GetKey(KeyCode.LeftShift)) {
            gridView.SetBoxSelectionEnd(end.x + rowChange, end.y + columnChange);
            gridView.FocusSelectionEnd();
        }
        else {
            gridView.ClearBoxSelection();
            
            if (!Input.GetKey(KeyCode.LeftControl))
                gridView.ClearSelection();

            if (rowSelecting)
                end.y = 0;
            
            gridView.SetBoxSelectionStart(end.x + rowChange, end.y + columnChange);
            gridView.FocusSelectionStart();
        }

        rowSelecting = false;
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

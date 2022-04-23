using System;
using TMPro;
using UnityEngine;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;

    private bool dragging;
    private bool rowSelecting;

    private void Awake() {
        gridView.DragBegin += OnGridDragBegin;
        gridView.DragUpdate += OnGridDragUpdate;
        gridView.DragEnd += OnGridDragEnd;
        gridView.SelectionStartChanged += OnSelectionStartChanged;
        gridView.BoxSelectionCancelled += OnBoxSelectionCancelled;
    }

    private void Start() {
        gridView.CreateEmpty(256, 6);
    }

    private void Update() {
        if (dragging)
            return;
        
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
        dragging = true;
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
        dragging = false;
    }

    private void OnSelectionStartChanged(int row, int column) {
        textField.interactable = true;
    }

    private void OnBoxSelectionCancelled() {
        textField.interactable = false;
    }
}

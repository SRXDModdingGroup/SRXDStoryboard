using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private int newDocumentRows;
    [SerializeField] private int newDocumentColumns;
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;

    private bool dragging;
    private bool rowSelecting;
    private bool anySelected;
    private bool anyBoxSelection;
    private StoryboardDocument document;
    private Table<CellVisualState> cellStates;
    private Vector2Int boxSelectionStart = new(-1, -1);
    private Vector2Int boxSelectionEnd = new (-1, -1);
    private EventSystem eventSystem;

    private void Awake() {
        gridView.DragBegin += OnGridDragBegin;
        gridView.DragUpdate += OnGridDragUpdate;
        gridView.DragEnd += OnGridDragEnd;
        eventSystem = EventSystem.current;
    }

    private void Start() {
        SetDocument(StoryboardDocument.CreateNew(newDocumentRows, newDocumentColumns));
        UpdateContent();
        UpdateSelection();
    }

    private void Update() {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected == textField.gameObject)
            UpdateTextFieldInput();
        else if (selected == gridView.gameObject && !dragging)
            UpdateGridViewInput();
    }

    #region Input

    private void UpdateTextFieldInput() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            eventSystem.SetSelectedGameObject(gridView.gameObject);

            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab)) {
            document.SetCellText(boxSelectionStart.x, boxSelectionStart.y, textField.text.Trim());
            UpdateContent();
            
            ClearSelection();
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, boxSelectionStart.y + 1);
            UpdateSelection();
            
            eventSystem.SetSelectedGameObject(gridView.gameObject);
            
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Return)) {
            FillSelectionWithValue(textField.text.Trim());
            
            int row = GetBottomOfSelection() + 1;

            if (Input.GetKey(KeyCode.LeftShift) || row >= cellStates.RowCount)
                document.InsertRow(row);

            UpdateContent();
            
            ClearSelection();
            SetBoxSelectionStartAndEnd(row, 0);
            UpdateSelection();
            
            eventSystem.SetSelectedGameObject(gridView.gameObject);
        }
    }

    private void UpdateGridViewInput() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            FocusTextField();
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            textField.SetTextWithoutNotify(string.Empty);
            FocusTextField();
            
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab)) {
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, boxSelectionStart.y + 1);
            UpdateSelection();
            
            eventSystem.SetSelectedGameObject(gridView.gameObject);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            ClearSelection();
            ClearBoxSelection();
            UpdateSelection();
            
            textField.interactable = false;
            eventSystem.SetSelectedGameObject(null);

            return;
        }
        
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString)) {
            textField.SetTextWithoutNotify(Input.inputString);
            FocusTextField();

            return;
        }

        bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow);
        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow);

        if (!leftPressed && !rightPressed && !upPressed && !downPressed)
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
            ApplyBoxSelection();

        if (Input.GetKey(KeyCode.LeftShift)) {
            SetBoxSelectionEnd(boxSelectionEnd.x + rowChange, boxSelectionEnd.y + columnChange);
            gridView.FocusSelectionEnd();
        }
        else {
            if (!Input.GetKey(KeyCode.LeftControl))
                ClearSelection();

            if (rowSelecting)
                boxSelectionEnd.y = 0;

            SetBoxSelectionStartAndEnd(boxSelectionEnd.x + rowChange, boxSelectionEnd.y + columnChange);
            gridView.FocusSelectionStart();
        }

        UpdateSelection();
        rowSelecting = false;
    }

    private void FocusTextField() {
        textField.caretPosition = textField.text.Length;
        textField.Select();
    }

    #endregion

    #region Interface

    private void SetDocument(StoryboardDocument document) {
        this.document = document;

        var content = document.Content;

        cellStates = new Table<CellVisualState>(content.RowCount, content.ColumnCount);

        for (int i = 0; i < cellStates.RowCount; i++) {
            for (int j = 0; j < cellStates.ColumnCount; j++)
                cellStates[i, j] = new CellVisualState();
        }
        
        gridView.SetCellStates(cellStates);
    }

    private void UpdateContent() {
        var content = document.Content;
        
        while (cellStates.RowCount < content.RowCount)
            cellStates.AddRow();
        
        while (cellStates.ColumnCount < content.ColumnCount)
            cellStates.AddColumn();

        while (cellStates.RowCount > content.RowCount)
            cellStates.RemoveLastRow();
        
        while (cellStates.ColumnCount > content.ColumnCount)
            cellStates.RemoveLastColumn();

        for (int i = 0; i < cellStates.RowCount; i++) {
            for (int j = 0; j < cellStates.ColumnCount; j++) {
                var cell = cellStates[i, j];

                if (cell == null) {
                    cell = new CellVisualState();
                    cellStates[i, j] = cell;
                }

                cell.Text = content[i, j].FormattedText;
            }
        }
        
        gridView.UpdateView();
    }

    private void FillSelectionWithValue(string value) {
        foreach (var cell in GetSelectedCells())
            document.SetCellText(cell.x, cell.y, value);
    }

    #endregion

    #region Selection

    private void SelectCell(int row, int column) => cellStates[row, column].Selected = true;

    private void DeselectCell(int row, int column) => cellStates[row, column].Selected = false;

    private void SetBoxSelectionStart(int row, int column) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, column));
        gridView.SetBoxSelectionStart(boxSelectionStart);
    }
    
    private void SetBoxSelectionEnd(int row, int column) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, column));
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }

    private void SetBoxSelectionStartAndEnd(int row, int column) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, column));
        boxSelectionEnd = boxSelectionStart;
        gridView.SetBoxSelectionStartAndEnd(boxSelectionStart);
    }
    
    private void SetRowSelectionStart(int row) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        boxSelectionEnd = boxSelectionStart;
        boxSelectionEnd.y = cellStates.ColumnCount - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }
    
    private void SetRowSelectionEnd(int row) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, cellStates.ColumnCount - 1));
        boxSelectionStart.y = 0;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }

    private void SetRowSelectionStartAndEnd(int row) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        boxSelectionEnd = boxSelectionStart;
        boxSelectionEnd.y = cellStates.ColumnCount - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }

    private void ApplyBoxSelection() {
        var clampedMin = ClampToBounds(Vector2Int.Min(boxSelectionStart, boxSelectionEnd));
        var clampedMax = ClampToBounds(Vector2Int.Max(boxSelectionStart, boxSelectionEnd));
                
        for (int i = clampedMin.x; i <= clampedMax.x; i++) {
            for (int j = clampedMin.y; j <= clampedMax.y; j++)
                SelectCell(i, j);
        }
    }

    private void ClearSelection() {
        for (int i = 0; i < cellStates.RowCount; i++) {
            for (int j = 0; j < cellStates.ColumnCount; j++)
                DeselectCell(i, j);
        }
    }

    private void ClearBoxSelection() {
        boxSelectionStart = new Vector2Int(-1, -1);
        boxSelectionEnd = boxSelectionStart;
        gridView.SetBoxSelectionStartAndEnd(boxSelectionStart);
    }

    private void UpdateSelection() {
        anyBoxSelection = IsInBounds(boxSelectionStart.x, boxSelectionStart.y);

        if (anyBoxSelection) {
            textField.SetTextWithoutNotify(document.Content[boxSelectionStart.x, boxSelectionStart.y].Text);
            textField.interactable = true;
            anySelected = true;
        }
        else {
            textField.SetTextWithoutNotify(string.Empty);
            textField.interactable = false;
            anySelected = false;
            
            for (int i = 0; i < cellStates.RowCount; i++) {
                for (int j = 0; j < cellStates.ColumnCount; j++) {
                    if (!IsInSelection(i, j))
                        continue;
                    
                    anySelected = true;
                        
                    break;
                }

                if (anySelected)
                    break;
            }
        }
        
        gridView.UpdateView();
    }

    private int GetBottomOfSelection() {
        int row = 0;
        
        foreach (var cell in GetSelectedCells()) {
            if (cell.x > row)
                row = cell.x;
        }

        return row;
    }
    
    #endregion

    #region Utility

    private bool IsInBounds(int row, int column) => row >= 0 && row < cellStates.RowCount && column >= 0 && column < cellStates.ColumnCount;
    
    private bool IsInSelection(int row, int column) {
        var boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        var boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        
        return IsInBounds(row, column) && (cellStates[row, column].Selected || row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
    }

    private Vector2Int ClampToBounds(Vector2Int index)
        => Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(index, new Vector2Int(cellStates.RowCount - 1, cellStates.ColumnCount - 1)));

    private IEnumerable<Vector2Int> GetSelectedCells() {
        for (int i = 0; i < cellStates.RowCount; i++) {
            for (int j = 0; j < cellStates.ColumnCount; j++) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    #endregion

    #region Events

    private void OnGridDragBegin(int row, int column) {
        eventSystem.SetSelectedGameObject(gridView.gameObject);
        dragging = true;
        rowSelecting = column < 0;
        
        if (Input.GetKey(KeyCode.LeftControl))
            ApplyBoxSelection();

        if (Input.GetKey(KeyCode.LeftShift)) {
            if (rowSelecting)
                SetRowSelectionEnd(row);
            else
                SetBoxSelectionEnd(row, column);
        }
        else {
            if (!Input.GetKey(KeyCode.LeftControl))
                ClearSelection();
            
            if (rowSelecting)
                SetRowSelectionStartAndEnd(row);
            else
                SetBoxSelectionStartAndEnd(row, column);
        }
        
        UpdateSelection();
    }
    
    private void OnGridDragUpdate(int row, int column) {
        if (rowSelecting)
            SetRowSelectionEnd(row);
        else
            SetBoxSelectionEnd(row, column);
        
        UpdateSelection();
    }
    
    private void OnGridDragEnd(int row, int column) {
        dragging = false;
        UpdateSelection();
    }

    #endregion
}

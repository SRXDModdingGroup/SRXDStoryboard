using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;

    private bool dragging;
    private bool rowSelecting;
    private bool anySelected;
    private int rowCount;
    private int columnCount;
    private List<List<CellState>> content;
    private Vector2Int boxSelectionStart = new(-1, -1);
    private Vector2Int boxSelectionEnd = new (-1, -1);
    private EventSystem eventSystem;

    private void Awake() {
        gridView.DragBegin += OnGridDragBegin;
        gridView.DragUpdate += OnGridDragUpdate;
        gridView.DragEnd += OnGridDragEnd;
        textField.onValueChanged.AddListener(OnTextFieldValueChanged);
        content = new List<List<CellState>>();
        eventSystem = EventSystem.current;
    }

    private void Start() {
        CreateEmpty(32, 8);
    }

    private void Update() {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected == textField.gameObject || selected == gridView.gameObject && !dragging)
            UpdateGeneralInput();
        
        if (selected == textField.gameObject)
            UpdateTextFieldInput();
        else if (selected == gridView.gameObject && !dragging)
            UpdateGridViewInput();
    }

    private void UpdateGeneralInput() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, boxSelectionStart.y + 1);
            
            return;
        }
    }

    private void UpdateTextFieldInput() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            eventSystem.SetSelectedGameObject(gridView.gameObject);

            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Return)) {
            eventSystem.SetSelectedGameObject(gridView.gameObject);

            string trimmed = textField.text.Trim();

            foreach (var cell in GetSelectedCells())
                SetCellText(cell.x, cell.y, trimmed);

            ClearSelection();

            int row = GetBottomOfSelection() + 1;

            if (Input.GetKey(KeyCode.LeftShift))
                InsertRow(row);

            SetBoxSelectionStartAndEnd(row, 0);
        }
    }

    private void UpdateGridViewInput() {
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString)) {
            textField.Select();
            textField.text = Input.inputString;
            textField.caretPosition = textField.text.Length;

            return;
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            FocusTextField();
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            ClearSelection();
            eventSystem.SetSelectedGameObject(null);
            textField.interactable = false;

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

        rowSelecting = false;
    }

    private void CreateEmpty(int rowCount, int columnCount) {
        this.rowCount = rowCount;
        this.columnCount = columnCount;
        
        while (content.Count < rowCount)
            content.Add(new List<CellState>());

        foreach (var row in content) {
            row.Clear();

            for (int i = 0; i < columnCount; i++)
                row.Add(new CellState());
        }
        
        gridView.SetContent(content, rowCount, columnCount);
    }

    private void FocusTextField() {
        textField.caretPosition = textField.text.Length;
        textField.Select();
    }

    private void SetCellText(int row, int column, string value) {
        content[row][column].Text = value;
        content[row][column].DisplayedText = value;
        gridView.UpdateView();
    }

    private void InsertRow(int index) {
        var row = new List<CellState>();

        for (int i = 0; i < columnCount; i++)
            row.Add(new CellState());
        
        content.Insert(index, row);
        gridView.InsertRow(index);
        rowCount++;
        gridView.SetContent(content, rowCount, columnCount);
    }
    
    private void SelectCell(int row, int column) => content[row][column].Selected = true;

    private void DeselectCell(int row, int column) => content[row][column].Selected = false;

    private void SetBoxSelectionStart(int row, int column) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, column));
        gridView.SetBoxSelectionStart(boxSelectionStart);
        UpdateSelection();
    }
    
    private void SetBoxSelectionEnd(int row, int column) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, column));
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
        UpdateSelection();
    }

    private void SetBoxSelectionStartAndEnd(int row, int column) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, column));
        boxSelectionEnd = boxSelectionStart;
        gridView.SetBoxSelectionStartAndEnd(boxSelectionStart);
        UpdateSelection();
    }
    
    private void SetRowSelectionStart(int row) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        boxSelectionEnd = boxSelectionStart;
        boxSelectionEnd.y = columnCount - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
        UpdateSelection();
    }
    
    private void SetRowSelectionEnd(int row) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, columnCount - 1));
        boxSelectionStart.y = 0;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
        UpdateSelection();
    }

    private void SetRowSelectionStartAndEnd(int row) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        boxSelectionEnd = boxSelectionStart;
        boxSelectionEnd.y = columnCount - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
        UpdateSelection();
    }

    private void UpdateSelection() {
        textField.SetTextWithoutNotify(content[boxSelectionStart.x][boxSelectionStart.y].Text);
        gridView.UpdateView();
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
        if (rowCount == 0 || columnCount == 0)
            return;
        
        for (int i = 0; i < rowCount; i++) {
            for (int j = 0; j < columnCount; j++)
                DeselectCell(i, j);
        }
    }

    private int GetBottomOfSelection() {
        int row = 0;
        
        foreach (var cell in GetSelectedCells()) {
            if (cell.x > row)
                row = cell.x;
        }

        return row;
    }
    
    private bool IsInBounds(int row, int column) => row >= 0 && row < rowCount && column >= 0 && column < columnCount;
    
    private bool IsInSelection(int row, int column) {
        var boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        var boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        
        return IsInBounds(row, column) && (content[row][column].Selected || row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
    }

    private Vector2Int ClampToBounds(Vector2Int index)
        => Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(index, new Vector2Int(rowCount - 1, columnCount - 1)));

    private IEnumerable<Vector2Int> GetSelectedCells() {
        for (int i = 0; i < rowCount; i++) {
            for (int j = 0; j < columnCount; j++) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

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

        textField.interactable = true;
    }
    
    private void OnGridDragUpdate(int row, int column) {
        if (rowSelecting)
            SetRowSelectionEnd(row);
        else
            SetBoxSelectionEnd(row, column);
    }
    
    private void OnGridDragEnd(int row, int column) => dragging = false;

    private void OnSelectionStartChanged(int row, int column) {
        textField.SetTextWithoutNotify(content[row][column].Text);
        textField.interactable = true;
    }

    private void OnAnyBoxSelectionChanged(bool value) => textField.interactable = value;

    private void OnTextFieldValueChanged(string value) {
        SetCellText(boxSelectionStart.x, boxSelectionStart.y, value.Trim());
    }
}

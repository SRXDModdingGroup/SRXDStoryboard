using System;
using System.Collections.Generic;
using System.Text;
using StoryboardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private int newDocumentRows;
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;

    private bool dragging;
    private bool rowSelecting;
    private bool anySelected;
    private bool anyBoxSelection;
    private StoryboardDocument document;
    private DocumentAnalysis analysis;
    private Table<CellVisualState> cellStates;
    private Vector2Int boxSelectionStart = new(-1, -1);
    private Vector2Int boxSelectionEnd = new (-1, -1);
    private EventSystem eventSystem;

    private void Awake() {
        gridView.DragBegin += OnGridDragBegin;
        gridView.DragUpdate += OnGridDragUpdate;
        gridView.DragEnd += OnGridDragEnd;
        gridView.Deselected += OnGridDeselected;
        eventSystem = EventSystem.current;
        analysis = new DocumentAnalysis();
    }

    private void Start() {
        if (StoryboardDocument.TryOpenFile("C:/Users/domia/OneDrive/My Charts/Storyboards/We Could Get More Machinegun Psystyle!.txt", out var document)) {
            SetDocument(document);
            document.SaveToFile("C:/Users/domia/OneDrive/My Charts/Storyboards/We Could Get More Machinegun Psystyle!.txt");
        }
        else
            SetDocument(StoryboardDocument.CreateNew(newDocumentRows));
        
        UpdateContent();
        UpdateSelection();
    }

    private void Update() {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected == textField.gameObject)
            UpdateTextFieldInput();
        else if (selected == gridView.gameObject && anyBoxSelection && !dragging)
            UpdateGridViewInput();
    }

    #region Input

    private void UpdateTextFieldInput() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(textField.text));
            
            var rightmostPerRow = new List<Vector2Int>(GetRightmostSelectedPerRow());

            if (ShiftHeld()) {
                foreach (var index in rightmostPerRow) {
                    if (index.y < document.Content.Columns - 1)
                        InsertAndPushCellsRight(index.x, index.y + 1, string.Empty);
                }
            }
                
            EndEdit();

            ClearSelection();

            foreach (var index in rightmostPerRow) {
                if (index.y < document.Content.Columns - 1)
                    cellStates[index.x, index.y + 1].Selected = true;
            }
            
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, GetRightmostSelectedInRow(boxSelectionStart.x));
            UpdateSelection();
            
            eventSystem.SetSelectedGameObject(gridView.gameObject);
            
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Return)) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(textField.text));
            
            int row = GetBottomOfSelection() + 1;

            if (ShiftHeld() || row >= cellStates.Rows)
                document.InsertRow(row);
            
            EndEdit();
            
            ClearSelection();
            MoveSelectionBackToLastEmpty(row, boxSelectionStart.y);
            UpdateSelection();
            
            eventSystem.SetSelectedGameObject(gridView.gameObject);

            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)) {
            UnfocusTextField();
            eventSystem.SetSelectedGameObject(gridView.gameObject);
            
            UpdateSelection();

            return;
        }
    }

    private void UpdateGridViewInput() {
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            BeginEdit();

            if (ShiftHeld()) {
                foreach (var index in GetSelectedCellsReversed())
                    DeleteAndPullCellsLeft(index.x, index.y);
            }
            else
                FillSelectionWithValue(string.Empty);

            if (!AnyInSelectedRows()) {
                foreach (int row in GetSelectedRowsReversed())
                    document.RemoveRow(row);
                
                EndEdit();

                int top = Math.Max(0, GetTopOfSelection() - 1);

                SetBoxSelectionStartAndEnd(top, GetRightmostFilledInRow(top));
                ClearSelection();
                UpdateSelection();
                
                return;
            }

            EndEdit();
            
            var leftmostPerRow = new List<Vector2Int>(GetLeftmostSelectedPerRow());
            
            ClearSelection();
            
            foreach (var index in leftmostPerRow) {
                if (IsInBounds(index.x, index.y - 1))
                    cellStates[index.x, index.y - 1].Selected = true;
            }
            
            
            
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, GetLeftmostSelectedInRow(boxSelectionStart.x));
            UpdateSelection();
            
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab)) {
            var rightmostPerRow = new List<Vector2Int>(GetRightmostSelectedPerRow());

            if (ShiftHeld()) {
                BeginEdit();
                
                foreach (var index in rightmostPerRow) {
                    if (index.y < document.Content.Columns - 1)
                        InsertAndPushCellsRight(index.x, index.y + 1, string.Empty);
                }
                
                EndEdit();
            }
            
            ClearSelection();

            foreach (var index in rightmostPerRow) {
                if (index.y < document.Content.Columns - 1)
                    cellStates[index.x, index.y + 1].Selected = true;
            }
            
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, GetRightmostSelectedInRow(boxSelectionStart.x));
            UpdateSelection();
            
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (!ShiftHeld()) {
                FocusTextField();

                return;
            }
            
            int row = GetBottomOfSelection() + 1;
            
            BeginEdit();
            document.InsertRow(row);
            EndEdit();
            
            ClearSelection();
            MoveSelectionBackToLastEmpty(row, boxSelectionStart.y);
            UpdateSelection();
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            ClearSelection();
            ClearBoxSelection();
            UpdateSelection();

            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            textField.SetTextWithoutNotify(string.Empty);
            FocusTextField();
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Delete)) {
            BeginEdit();
            
            if (ShiftHeld()) {
                foreach (var index in GetSelectedCellsReversed())
                    DeleteAndPullCellsLeft(index.x, index.y);
            }
            else
                FillSelectionWithValue(string.Empty);

            EndEdit();

            if (!ShiftHeld())
                return;
            
            var leftmostPerRow = new List<Vector2Int>(GetLeftmostSelectedPerRow());
                
            ClearSelection();
            
            foreach (var index in leftmostPerRow) {
                if (IsInBounds(index.x, index.y))
                    cellStates[index.x, index.y].Selected = true;
            }
            
            SetBoxSelectionStartAndEnd(boxSelectionStart.x, GetLeftmostSelectedInRow(boxSelectionStart.x));
            UpdateSelection();

            return;
        }
        
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString) && Input.inputString.Length == 1 && !char.IsControl(Input.inputString, 0)) {
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

        if (CtrlHeld())
            ApplyBoxSelection();

        if (ShiftHeld()) {
            SetBoxSelectionEnd(boxSelectionEnd.x + rowChange, boxSelectionEnd.y + columnChange);
            gridView.FocusSelectionEnd();
        }
        else {
            if (!CtrlHeld())
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
        textField.ActivateInputField();
        textField.caretPosition = textField.text.Length;
        textField.Select();
    }

    private void UnfocusTextField() {
        textField.DeactivateInputField(true);
        textField.ReleaseSelection();
        
        if (eventSystem.currentSelectedGameObject == textField.gameObject)
            eventSystem.SetSelectedGameObject(null);
    }

    private static bool ShiftHeld() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    private static bool CtrlHeld() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

    #endregion

    #region Logic

    private void SetDocument(StoryboardDocument document) {
        this.document = document;

        var content = document.Content;

        cellStates = new Table<CellVisualState>(content.Rows, content.Columns);

        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j] = new CellVisualState();
        }
        
        gridView.SetCellStates(cellStates);
    }

    private void BeginEdit() {
        analysis.Cancel();
        document.BeginEdit();
    }

    private void EndEdit() {
        document.EndEdit();
        UpdateContent();
        analysis.Analyse(document, UpdateContent);
    }

    private void UpdateContent() {
        var content = document.Content;
        
        cellStates.SetSize(content.Rows, content.Columns);

        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++) {
                var cell = cellStates[i, j];

                if (cell == null) {
                    cell = new CellVisualState();
                    cellStates[i, j] = cell;
                }

                cell.Text = content[i, j];
            }
        }

        if (anyBoxSelection)
            UpdateSelection();

        gridView.UpdateView();
    }

    private void FillSelectionWithValue(string value) {
        foreach (var cell in GetSelectedCells())
            document.SetCellText(cell.x, cell.y, value);
    }

    private void InsertAndPushCellsRight(int row, int column, string value) {
        var content = document.Content;
        
        for (int i = content.Columns - 1; i > column; i--)
            document.SetCellText(row, i, content[row, i - 1]);
        
        document.SetCellText(row, column, value);
    }
    
    private void DeleteAndPullCellsLeft(int row, int column) {
        var content = document.Content;
        
        for (int i = column; i < content.Columns - 1; i++)
            document.SetCellText(row, i, content[row, i + 1]);
        
        document.SetCellText(row, content.Columns - 1, string.Empty);
    }

    private static string AutoFormat(string value, bool outermost = true) {
        var builder = new StringBuilder();
        int length = value.Length;

        for (int i = 0; i < length; i++) {
            char c = value[i];
            
            switch (c) {
                case '\"':
                    builder.Append('\"');
                    
                    int start = i + 1;

                    Parser.TrySkipTo(value, ref i, '\"');
                    
                    if (i > start)
                        builder.Append(AutoFormat(value.Substring(start, i - start), false));

                    builder.Append('\"');

                    continue;
                case '(':
                    SkipToAndFormat('(', ')');
                    continue;
                case '{':
                    SkipToAndFormat('{', '}');
                    continue;
                case '[':
                    SkipToAndFormat('[', ']');
                    continue;
                case ' ' when outermost:
                case ')':
                case '}':
                case ']':
                    continue;
                default:
                    builder.Append(c);
                    continue;
            }

            void SkipToAndFormat(char start, char end) {
                builder.Append(start);

                int idx = i + 1;

                Parser.TrySkipTo(value, ref i, start, end);
                
                if (i > idx)
                    builder.Append(AutoFormat(value.Substring(idx, i - idx), false));

                builder.Append(end);
            }
        }

        return builder.ToString();
    }

    #endregion

    #region Selection

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
        boxSelectionEnd.y = cellStates.Columns - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }
    
    private void SetRowSelectionEnd(int row) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, cellStates.Columns - 1));
        boxSelectionStart.y = 0;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }

    private void SetRowSelectionStartAndEnd(int row) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        boxSelectionEnd = boxSelectionStart;
        boxSelectionEnd.y = cellStates.Columns - 1;
        gridView.SetBoxSelectionStart(boxSelectionStart);
        gridView.SetBoxSelectionEnd(boxSelectionEnd);
    }

    private void ApplyBoxSelection() {
        var clampedMin = ClampToBounds(Vector2Int.Min(boxSelectionStart, boxSelectionEnd));
        var clampedMax = ClampToBounds(Vector2Int.Max(boxSelectionStart, boxSelectionEnd));
                
        for (int i = clampedMin.x; i <= clampedMax.x; i++) {
            for (int j = clampedMin.y; j <= clampedMax.y; j++)
                cellStates[i, j].Selected = true;
        }
    }

    private void MoveSelectionBackToLastEmpty(int row, int column) {
        var content = document.Content;

        if (string.IsNullOrWhiteSpace(content[row, column])) {
            while (column > 0 && string.IsNullOrWhiteSpace(content[row, column - 1]))
                column--;
        }

        SetBoxSelectionStartAndEnd(row, column);
    }

    private void ClearSelection() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j].Selected = false;
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
            boxSelectionStart = ClampToBounds(boxSelectionStart);
            boxSelectionEnd = ClampToBounds(boxSelectionEnd);
            
            if (eventSystem.currentSelectedGameObject != textField.gameObject)
                textField.SetTextWithoutNotify(document.Content[boxSelectionStart.x, boxSelectionStart.y]);
            
            textField.interactable = true;
            anySelected = true;
        }
        else {
            boxSelectionStart = new Vector2Int(-1, -1);
            boxSelectionEnd = boxSelectionStart;
            gridView.SetBoxSelectionStartAndEnd(boxSelectionStart);
            textField.SetTextWithoutNotify(string.Empty);
            UnfocusTextField();
            textField.interactable = false;
            
            if (eventSystem.currentSelectedGameObject == textField.gameObject
                || eventSystem.currentSelectedGameObject == gridView.gameObject)
                eventSystem.SetSelectedGameObject(null);
            
            anySelected = false;
            
            for (int i = 0; i < cellStates.Rows; i++) {
                for (int j = 0; j < cellStates.Columns; j++) {
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
    
    private bool IsInSelection(int row, int column) {
        var boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        var boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        
        return IsInBounds(row, column) && (cellStates[row, column].Selected || row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
    }

    private bool AnyInSelectedRows() {
        foreach (int row in GetSelectedRows()) {
            if (AnyInRow(row))
                return true;
        }

        return false;
    }

    private int GetTopOfSelection() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++) {
                if (IsInSelection(i, j))
                    return i;
            }
        }

        return 0;
    }

    private int GetBottomOfSelection() {
        for (int i = cellStates.Rows - 1; i >= 0; i--) {
            for (int j = 0; j < cellStates.Columns; j++) {
                if (IsInSelection(i, j))
                    return i;
            }
        }

        return 0;
    }

    private int GetRightmostSelectedInRow(int row) {
        int rightmost = -1;
            
        for (int j = 0; j < cellStates.Columns; j++) {
            if (IsInSelection(row, j))
                rightmost = j;
        }

        if (rightmost >= 0)
            return rightmost;

        return -1;
    }

    private int GetLeftmostSelectedInRow(int row) {
        int leftmost = -1;
            
        for (int j = 0; j < cellStates.Columns; j++) {
            if (!IsInSelection(row, j))
                continue;
            
            leftmost = j;

            break;
        }

        if (leftmost >= 0)
            return leftmost;

        return -1;
    }

    private IEnumerable<int> GetSelectedRows() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++) {
                if (!IsInSelection(i, j))
                    continue;
                
                yield return i;

                break;
            }
        }
    }

    private IEnumerable<int> GetSelectedRowsReversed() {
        for (int i = cellStates.Rows - 1; i >= 0; i--) {
            for (int j = 0; j < cellStates.Columns; j++) {
                if (!IsInSelection(i, j))
                    continue;
                
                yield return i;

                break;
            }
        }
    }

    private IEnumerable<Vector2Int> GetSelectedCells() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    private IEnumerable<Vector2Int> GetSelectedCellsReversed() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = cellStates.Columns - 1; j >= 0; j--) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    private IEnumerable<Vector2Int> GetRightmostSelectedPerRow() {
        for (int i = 0; i < cellStates.Rows; i++) {
            int rightmost = GetRightmostSelectedInRow(i);

            if (IsInBounds(i, rightmost))
                yield return new Vector2Int(i, rightmost);
        }
    }

    private IEnumerable<Vector2Int> GetLeftmostSelectedPerRow() {
        for (int i = 0; i < cellStates.Rows; i++) {
            int leftmost = GetLeftmostSelectedInRow(i);

            if (IsInBounds(i, leftmost))
                yield return new Vector2Int(i, leftmost);
        }
    }

    #endregion

    #region Utility

    private bool IsInBounds(int row, int column) => row >= 0 && row < cellStates.Rows && column >= 0 && column < cellStates.Columns;
    
    private bool AnyInRow(int row) {
        var content = document.Content;

        for (int i = 0; i < content.Columns; i++) {
            if (!string.IsNullOrWhiteSpace(content[row, i]))
                return true;
        }

        return false;
    }

    private int GetRightmostFilledInRow(int row) {
        var content = document.Content;

        for (int i = content.Columns - 1; i >= 0; i--) {
            if (!string.IsNullOrWhiteSpace(content[row, i]))
                return i;
        }

        return -1;
    }

    private Vector2Int ClampToBounds(Vector2Int index)
        => Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(index, new Vector2Int(cellStates.Rows - 1, cellStates.Columns - 1)));

    #endregion

    #region Events

    private void OnGridDragBegin(int row, int column) {
        textField.DeactivateInputField(true);
        textField.ReleaseSelection();
        eventSystem.SetSelectedGameObject(gridView.gameObject);
        dragging = true;
        rowSelecting = column < 0;
        
        if (CtrlHeld())
            ApplyBoxSelection();

        if (ShiftHeld()) {
            if (rowSelecting)
                SetRowSelectionEnd(row);
            else
                SetBoxSelectionEnd(row, column);
        }
        else {
            if (!CtrlHeld())
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

    private void OnGridDeselected() {
        dragging = false;
        rowSelecting = false;
    }

    #endregion
}

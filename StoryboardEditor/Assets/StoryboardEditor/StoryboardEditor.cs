using System;
using System.Collections.Generic;
using System.Text;
using StoryboardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static EditorInput;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private int newDocumentRows;
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;

    private bool contentNeedsUpdate;
    private bool dragging;
    private bool rowSelecting;
    private bool anySelected;
    private bool anyBoxSelection;
    private bool editing;
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

        var input = GetComponent<EditorInput>();

        input.Backspace += OnBackspace;
        input.Tab += OnTab;
        input.Return += OnReturn;
        input.Escape += OnEscape;
        input.Space += OnSpace;
        input.Delete += OnDelete;
        input.Direction += OnDirection;
        input.Character += OnCharacter;
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
        analysis.Analyse(document, () => contentNeedsUpdate = true);
    }

    private void Update() {
        if (contentNeedsUpdate)
            UpdateContent();

        contentNeedsUpdate = false;
    }

    #region Input

    private void OnBackspace(InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        BeginEdit();

        if (modifiers.HasModifiers(InputModifier.Shift)) {
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
    }

    private void OnTab(InputModifier modifiers) {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected != gridView.gameObject && selected != textField.gameObject)
            return;

        if (selected == textField.gameObject) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(textField.text));
        }
        
        var rightmostPerRow = new List<Vector2Int>(GetRightmostSelectedPerRow());

        if (modifiers.HasModifiers(InputModifier.Shift)) {
            BeginEdit();
                
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
    }

    private void OnReturn(InputModifier modifiers) {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected != gridView.gameObject && selected != textField.gameObject)
            return;
        
        if (selected == gridView.gameObject && !modifiers.HasModifiers(InputModifier.Shift)) {
            FocusTextField();

            return;
        }
        
        if (selected == textField.gameObject) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(textField.text));
        }

        int row = GetBottomOfSelection() + 1;

        if (modifiers.HasModifiers(InputModifier.Shift) || row >= cellStates.Rows) {
            BeginEdit();
            document.InsertRow(row);
        }
        
        EndEdit();
            
        ClearSelection();
        MoveSelectionBackToLastEmpty(row, boxSelectionStart.y);
        UpdateSelection();
        
        eventSystem.SetSelectedGameObject(gridView.gameObject);
    }

    private void OnEscape(InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject == textField.gameObject) {
            UnfocusTextField();
            eventSystem.SetSelectedGameObject(gridView.gameObject);
            
            UpdateSelection();
        }
        else if (eventSystem.currentSelectedGameObject == gridView.gameObject) {
            ClearSelection();
            ClearBoxSelection();
            UpdateSelection();
        }
    }

    private void OnSpace(InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        textField.SetTextWithoutNotify(string.Empty);
        FocusTextField();
    }

    private void OnDelete(InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        BeginEdit();
            
        if (modifiers.HasModifiers(InputModifier.Shift)) {
            foreach (var index in GetSelectedCellsReversed())
                DeleteAndPullCellsLeft(index.x, index.y);
        }
        else
            FillSelectionWithValue(string.Empty);

        EndEdit();

        if (!modifiers.HasModifiers(InputModifier.Shift))
            return;
            
        var leftmostPerRow = new List<Vector2Int>(GetLeftmostSelectedPerRow());
                
        ClearSelection();
            
        foreach (var index in leftmostPerRow) {
            if (IsInBounds(index.x, index.y))
                cellStates[index.x, index.y].Selected = true;
        }
            
        SetBoxSelectionStartAndEnd(boxSelectionStart.x, GetLeftmostSelectedInRow(boxSelectionStart.x));
        UpdateSelection();
    }

    private void OnDirection(Vector2Int direction, InputModifier modifiers) {
        if (modifiers.HasModifiers(InputModifier.Control))
            ApplyBoxSelection();

        if (modifiers.HasModifiers(InputModifier.Shift)) {
            SetBoxSelectionEnd(boxSelectionEnd.x + direction.x, boxSelectionEnd.y + direction.y);
            gridView.FocusSelectionEnd();
        }
        else {
            if (!modifiers.HasModifiers(InputModifier.Control))
                ClearSelection();

            if (rowSelecting)
                boxSelectionEnd.y = 0;

            SetBoxSelectionStartAndEnd(boxSelectionEnd.x + direction.x, boxSelectionEnd.y + direction.y);
            gridView.FocusSelectionStart();
        }

        UpdateSelection();
        rowSelecting = false;
    }

    private void OnCharacter(string character, InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        textField.SetTextWithoutNotify(character);
        FocusTextField();
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
        if (editing)
            return;

        editing = true;
        analysis.Cancel();
        document.BeginEdit();
    }

    private void EndEdit() {
        if (!editing)
            return;

        editing = false;
        document.EndEdit();
        contentNeedsUpdate = true;
        analysis.Analyse(document, () => contentNeedsUpdate = true);
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

                var info = analysis.Cells;

                if (i >= info.Rows || j >= info.Columns) {
                    cell.FormattedText = cell.Text;
                    
                    continue;
                }

                var infoCell = info[i, j];

                if (string.IsNullOrWhiteSpace(infoCell.FormattedText) || cell.Text != infoCell.Text)
                    cell.FormattedText = cell.Text;
                else
                    cell.FormattedText = infoCell.FormattedText;
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
        value = value.Trim();
        
        if (value.StartsWith("//"))
            return value;
        
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

    private void OnGridDragBegin(int row, int column, InputModifier modifiers) {
        textField.DeactivateInputField(true);
        textField.ReleaseSelection();
        eventSystem.SetSelectedGameObject(gridView.gameObject);
        dragging = true;
        rowSelecting = column < 0;
        
        if (modifiers.HasModifiers(InputModifier.Control))
            ApplyBoxSelection();

        if (modifiers.HasModifiers(InputModifier.Shift)) {
            if (rowSelecting)
                SetRowSelectionEnd(row);
            else
                SetBoxSelectionEnd(row, column);
        }
        else {
            if (!modifiers.HasModifiers(InputModifier.Control))
                ClearSelection();
            
            if (rowSelecting)
                SetRowSelectionStartAndEnd(row);
            else
                SetBoxSelectionStartAndEnd(row, column);
        }
        
        UpdateSelection();
    }
    
    private void OnGridDragUpdate(int row, int column, InputModifier modifiers) {
        if (rowSelecting)
            SetRowSelectionEnd(row);
        else
            SetBoxSelectionEnd(row, column);
        
        UpdateSelection();
    }
    
    private void OnGridDragEnd(int row, int column, InputModifier modifiers) {
        dragging = false;
        UpdateSelection();
    }

    private void OnGridDeselected(InputModifier modifiers) {
        dragging = false;
        rowSelecting = false;
    }

    #endregion
}

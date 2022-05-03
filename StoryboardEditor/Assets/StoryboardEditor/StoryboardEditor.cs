using System;
using System.Collections.Generic;
using System.Text;
using StoryboardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private int newDocumentRows;
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GridView gridView;
    [SerializeField] private TopBarButton[] topBarButtons;
    [SerializeField] private TextInputPopup textInputPopup;
    [SerializeField] private InputBlocker blocker;

    private bool contentNeedsUpdate;
    private bool editing;
    private bool rowSelecting;
    private Table<string> document;
    private Table<string> clipboard;
    private Table<CellVisualState> cellStates;
    private EventSystem eventSystem;
    private EditorSettings settings;
    private EditorInput input;
    private EditorSelection selection;
    private EditorAnalysis analysis;
    private UndoRedo undoRedo;

    private void Awake() {
        gridView.DragBegin += OnGridDragBegin;
        gridView.DragUpdate += OnGridDragUpdate;
        gridView.DragEnd += OnGridDragEnd;
        gridView.Deselected += OnGridDeselected;
        eventSystem = EventSystem.current;
        settings = new EditorSettings();
        input = new EditorInput();
        selection = new EditorSelection();
        analysis = new EditorAnalysis();
        undoRedo = new UndoRedo();

        input.Backspace += OnBackspace;
        input.Tab += OnTab;
        input.Return += OnReturn;
        input.Escape += OnEscape;
        input.Space += OnSpace;
        input.Delete += OnDelete;
        input.Direction += OnDirection;
        input.Character += OnCharacter;
        input.Bind(BindableAction.Undo, Undo);
        input.Bind(BindableAction.Redo, Redo);
        input.Bind(BindableAction.Copy, Copy);
        input.Bind(BindableAction.Paste, () => Paste(false));
        input.Bind(BindableAction.PasteAndInsert, () => Paste(true));
        input.Bind(BindableAction.Rename, Rename);
    }

    private void Start() {
        foreach (var topBarButton in topBarButtons) {
            topBarButton.Init(GetTopBarButtonValues(topBarButton), input.Execute, blocker);

            IEnumerable<ContextMenu.ItemValue> GetTopBarButtonValues(TopBarButton topBarButton) {
                foreach (var bindableAction in topBarButton.Actions) {
                    var binding = settings.Bindings[bindableAction];
                    var builder = new StringBuilder();
                    var modifiers = binding.Modifiers;

                    if (modifiers.HasAnyModifiers(InputModifier.Control))
                        builder.Append("Ctrl+");
                
                    if (modifiers.HasAnyModifiers(InputModifier.Alt))
                        builder.Append("Alt+");
                
                    if (modifiers.HasAnyModifiers(InputModifier.Shift))
                        builder.Append("Shift+");

                    builder.Append(binding.InputString);

                    yield return new ContextMenu.ItemValue(binding.Name, builder.ToString(), CanExecuteAction(bindableAction));
                }
            }
        }
        
        if (StoryboardDocument.TryOpenFile("C:/Users/domia/OneDrive/My Charts/Storyboards/We Could Get More Machinegun Psystyle!.txt", out document)) {
            SetDocument(document);
            StoryboardDocument.SaveToFile(document, "C:/Users/domia/OneDrive/My Charts/Storyboards/We Could Get More Machinegun Psystyle!.txt");
        }
        else
            SetDocument(StoryboardDocument.CreateNew(newDocumentRows));
        
        UpdateBounds();
        UpdateContent();
        UpdateSelection();
        analysis.Analyze(document, () => contentNeedsUpdate = true);
    }

    private void Update() {
        input.UpdateInput(settings);
        
        if (contentNeedsUpdate)
            UpdateContent();

        contentNeedsUpdate = false;
    }

    #region Input

    private void OnBackspace(InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        BeginEdit();

        if (modifiers.HasAllModifiers(InputModifier.Shift)) {
            foreach (var index in selection.GetSelectedCellsReversed())
                DeleteAndPullCellsLeft(index.x, index.y);
        }
        else
            FillSelectionWithValue(string.Empty);

        if (!AnyInSelectedRows()) {
            int top = Math.Max(0, selection.GetTopOfSelection() - 1);
            
            foreach (int row in selection.GetSelectedRowsReversed())
                RemoveRow(row);
                
            EndEdit();

            selection.SetBoxSelectionStartAndEnd(top, GetRightmostFilledInRow(top));
            selection.ClearSelection();
            UpdateSelection();
                
            return;
        }

        EndEdit();
            
        var leftmostPerRow = new List<Vector2Int>(selection.GetLeftmostSelectedPerRow());
            
        selection.ClearSelection();
            
        foreach (var index in leftmostPerRow)
            selection.Select(index.x, index.y - 1);

        selection.SetBoxSelectionStartAndEnd(selection.BoxSelectionStart.x, selection.GetLeftmostSelectedInRow(selection.BoxSelectionStart.x));
        UpdateSelection();
    }

    private void OnTab(InputModifier modifiers) {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected != gridView.gameObject && selected != textField.gameObject)
            return;

        if (selected == textField.gameObject) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(new StringRange(textField.text)));
        }
        
        var rightmostPerRow = new List<Vector2Int>(selection.GetRightmostSelectedPerRow());
        bool insertNew = false;
        int insertOffset = 1;

        if (modifiers.HasExactModifiers(InputModifier.Control)) {
            insertNew = true;
            insertOffset = 0;
        }

        if (modifiers.HasExactModifiers(InputModifier.Shift)) {
            insertNew = true;
            insertOffset = 1;
        }

        if (insertNew) {
            BeginEdit();
                
            foreach (var index in rightmostPerRow) {
                if (index.y < document.Columns - 1)
                    InsertAndPushCellsRight(index.x, index.y + insertOffset, string.Empty);
            }
        }
        
        EndEdit();
            
        selection.ClearSelection();

        foreach (var index in rightmostPerRow)
            selection.Select(index.x, index.y + insertOffset);

        selection.SetBoxSelectionStartAndEnd(selection.BoxSelectionStart.x, selection.GetRightmostSelectedInRow(selection.BoxSelectionStart.x));
        UpdateSelection();
        
        eventSystem.SetSelectedGameObject(gridView.gameObject);
    }

    private void OnReturn(InputModifier modifiers) {
        var selected = eventSystem.currentSelectedGameObject;
        
        if (selected != gridView.gameObject && selected != textField.gameObject)
            return;
        
        bool insertNew = false;
        int insertOffset = 1;

        if (modifiers.HasExactModifiers(InputModifier.Control)) {
            insertNew = true;
            insertOffset = 0;
        }

        if (modifiers.HasExactModifiers(InputModifier.Shift)) {
            insertNew = true;
            insertOffset = 1;
        }
        
        if (!insertNew && selected == gridView.gameObject) {
            FocusTextField();

            return;
        }
        
        if (selected == textField.gameObject) {
            UnfocusTextField();
            BeginEdit();
            FillSelectionWithValue(AutoFormat(new StringRange(textField.text)));
        }

        int row = selection.GetBottomOfSelection() + insertOffset;

        if (insertNew || row >= document.Rows) {
            BeginEdit();
            InsertRow(row);
        }
        
        EndEdit();
            
        selection.ClearSelection();
        selection.SetBoxSelectionStartAndEnd(row, Math.Min(selection.BoxSelectionStart.y, GetRightmostFilledInRow(row) + 1));
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
            selection.ClearSelection();
            selection.ClearBoxSelection();
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
            
        if (modifiers.HasAllModifiers(InputModifier.Shift)) {
            foreach (var index in selection.GetSelectedCellsReversed())
                DeleteAndPullCellsLeft(index.x, index.y);
        }
        else
            FillSelectionWithValue(string.Empty);

        EndEdit();

        if (modifiers.HasAllModifiers(InputModifier.Shift)) {
            var leftmostPerRow = new List<Vector2Int>(selection.GetLeftmostSelectedPerRow());

            selection.ClearSelection();

            foreach (var index in leftmostPerRow)
                selection.Select(index.x, index.y);

            selection.SetBoxSelectionStartAndEnd(selection.BoxSelectionStart.x, selection.GetLeftmostSelectedInRow(selection.BoxSelectionStart.x));
        }
        
        UpdateSelection();
    }

    private void OnDirection(Vector2Int direction, InputModifier modifiers) {
        if (eventSystem.currentSelectedGameObject != gridView.gameObject)
            return;
        
        if (modifiers.HasAllModifiers(InputModifier.Control))
            selection.ApplyBoxSelection();

        if (modifiers.HasAllModifiers(InputModifier.Shift)) {
            selection.SetBoxSelectionEnd(selection.BoxSelectionEnd.x + direction.x, selection.BoxSelectionEnd.y + direction.y);
            gridView.FocusSelectionEnd();
        }
        else {
            if (!modifiers.HasAllModifiers(InputModifier.Control))
                selection.ClearSelection();

            selection.SetBoxSelectionStartAndEnd(selection.BoxSelectionEnd.x + direction.x, selection.BoxSelectionEnd.y + direction.y);
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

    private void OnGridDragBegin(int row, int column, InputModifier modifiers) {
        textField.DeactivateInputField(true);
        textField.ReleaseSelection();
        eventSystem.SetSelectedGameObject(gridView.gameObject);
        rowSelecting = column < 0;
        
        if (modifiers.HasAllModifiers(InputModifier.Control))
            selection.ApplyBoxSelection();

        if (modifiers.HasAllModifiers(InputModifier.Shift)) {
            if (rowSelecting)
                selection.SetRowSelectionEnd(row);
            else
                selection.SetBoxSelectionEnd(row, column);
        }
        else {
            if (!modifiers.HasAllModifiers(InputModifier.Control))
                selection.ClearSelection();
            
            if (rowSelecting)
                selection.SetRowSelectionStartAndEnd(row);
            else
                selection.SetBoxSelectionStartAndEnd(row, column);
        }
        
        UpdateSelection();
    }
    
    private void OnGridDragUpdate(int row, int column, InputModifier modifiers) {
        if (rowSelecting)
            selection.SetRowSelectionEnd(row);
        else
            selection.SetBoxSelectionEnd(row, column);
        
        UpdateSelection();
    }
    
    private void OnGridDragEnd(int row, int column, InputModifier modifiers) {
        UpdateSelection();
    }

    private void OnGridDeselected(InputModifier modifiers) {
        rowSelecting = false;
    }

    #endregion

    #region Logic

    private void SetDocument(Table<string> document) {
        this.document = document;
        StoryboardDocument.Optimize(document);
        cellStates = new Table<CellVisualState>(document.Rows, document.Columns);

        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j] = new CellVisualState();
        }
        
        gridView.Init(cellStates, selection);
        undoRedo.Clear();
    }

    private void BeginEdit() {
        if (editing)
            return;

        editing = true;
        analysis.Cancel();
        undoRedo.BeginNewAction();
    }

    private void EndEdit() {
        if (!editing)
            return;

        editing = false;
        StoryboardDocument.Optimize(document);
        UpdateBounds();
        analysis.Analyze(document, () => contentNeedsUpdate = true);
        undoRedo.CompleteAction();
        gridView.UpdateView();
    }

    private void SetCellText(int row, int column, string value) {
        string oldValue = document[row, column];
        
        Do(row, column, value);
        undoRedo.AddSubAction(() => Do(row, column, oldValue), () => Do(row, column, value));

        void Do(int row, int column, string value) {
            document.SetValueSafe(row, column, value);
            cellStates.SetValueSafe(row, column, new CellVisualState(value));
        }
    }

    private void InsertRow(int index) {
        Do(index);
        undoRedo.AddSubAction(() => Undo(index), () => Do(index));

        void Do(int index) {
            analysis.Cells.SetSize(document.Rows, document.Columns);
            document.InsertRow(index);
            cellStates.InsertRow(index);
            analysis.Cells.InsertRow(index);

            for (int i = 0; i < document.Columns; i++)
                document[index, i] = string.Empty;

            for (int i = 0; i < cellStates.Columns; i++)
                cellStates[index, i] = new CellVisualState();
        }

        void Undo(int index) {
            analysis.Cells.SetSize(document.Rows, document.Columns);
            document.RemoveRow(index);
            cellStates.RemoveRow(index);
            analysis.Cells.RemoveRow(index);
        }
    }

    private void RemoveRow(int index) {
        string[] oldContents = new string[document.Columns];

        for (int i = 0; i < document.Columns; i++)
            oldContents[i] = document[index, i];
        
        Do(index);
        undoRedo.AddSubAction(() => Undo(oldContents, index), () => Do(index));

        void Do(int index) {
            analysis.Cells.SetSize(document.Rows, document.Columns);
            document.RemoveRow(index);
            cellStates.RemoveRow(index);
            analysis.Cells.RemoveRow(index);
        }

        void Undo(string[] oldContents, int index) {
            analysis.Cells.SetSize(document.Rows, document.Columns);
            document.InsertRow(index);
            cellStates.InsertRow(index);
            analysis.Cells.InsertRow(index);

            for (int i = 0; i < oldContents.Length; i++) {
                string value = oldContents[i];

                document.SetValueSafe(index, i, value);
                cellStates.SetValueSafe(index, i, new CellVisualState(value));
            }
        }
    }

    private void Undo() {
        if (!undoRedo.CanUndo())
            return;
        
        analysis.Cancel();
        undoRedo.Undo();
        StoryboardDocument.Optimize(document);
        UpdateBounds();
        analysis.Analyze(document, () => contentNeedsUpdate = true);
        gridView.UpdateView();
    }
    
    private void Redo() {
        if (!undoRedo.CanRedo())
            return;
        
        analysis.Cancel();
        undoRedo.Redo();
        StoryboardDocument.Optimize(document);
        UpdateBounds();
        analysis.Analyze(document, () => contentNeedsUpdate = true);
        gridView.UpdateView();
    }

    private void UpdateBounds() {
        cellStates.SetSize(document.Rows, document.Columns);
        selection.SetSize(document.Rows, document.Columns);

        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j] ??= new CellVisualState();
        }
    }

    private void UpdateContent() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j].Text = document[i, j];
        }
        
        lock (analysis.Lock) {
            var info = analysis.Cells;

            for (int i = 0; i < cellStates.Rows; i++) {
                for (int j = 0; j < cellStates.Columns; j++) {
                    var cell = cellStates[i, j];

                    if (i >= info.Rows || j >= info.Columns) {
                        cell.FormattedText = cell.Text;
                        cell.IsError = false;

                        continue;
                    }

                    var infoCell = info[i, j];

                    if (string.IsNullOrWhiteSpace(infoCell.FormattedText))
                        cell.FormattedText = cell.Text;
                    else
                        cell.FormattedText = infoCell.FormattedText;

                    cell.IsError = infoCell.IsError;
                }
            }
        }

        if (selection.AnyBoxSelection)
            UpdateSelection();

        gridView.UpdateView();
    }
    
    private void UpdateSelection() {
        selection.UpdateSelection();

        if (selection.AnyBoxSelection) {
            if (eventSystem.currentSelectedGameObject != textField.gameObject)
                textField.SetTextWithoutNotify(document[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y]);
            
            textField.interactable = true;
        }
        else {
            textField.SetTextWithoutNotify(string.Empty);
            UnfocusTextField();
            textField.interactable = false;
            
            if (eventSystem.currentSelectedGameObject == textField.gameObject
                || eventSystem.currentSelectedGameObject == gridView.gameObject)
                eventSystem.SetSelectedGameObject(null);
        }
        
        gridView.UpdateView();
    }

    private void FillSelectionWithValue(string value) {
        foreach (var cell in selection.GetSelectedCells())
            SetCellText(cell.x, cell.y, value);
    }

    private void InsertAndPushCellsRight(int row, int column, string value) {
        for (int i = document.Columns - 1; i > column; i--)
            SetCellText(row, i, document[row, i - 1]);
        
        SetCellText(row, column, value);
    }
    
    private void DeleteAndPullCellsLeft(int row, int column) {
        for (int i = column; i < document.Columns - 1; i++)
            SetCellText(row, i, document[row, i + 1]);
        
        SetCellText(row, document.Columns - 1, string.Empty);
    }

    private void Copy() {
        if (!selection.AnySelected)
            return;

        var bounds = selection.GetSelectionBounds();

        if (clipboard == null)
            clipboard = new Table<string>(bounds.width, bounds.height);
        else
            clipboard.SetSize(bounds.width, bounds.height);

        for (int i = 0; i < bounds.width; i++) {
            int currentRow = bounds.x + i;
            
            for (int j = 0; j < bounds.height; j++) {
                int currentColumn = bounds.y + j;
                
                if (selection.IsInSelection(currentRow, currentColumn))
                    clipboard[i, j] = document[currentRow, currentColumn];
                else
                    clipboard[i, j] = null;
            }
        }
    }

    private void Paste(bool insert) {
        if (clipboard == null || !selection.AnyBoxSelection)
            return;
        
        BeginEdit();

        int row = selection.BoxSelectionStart.x;
        int column = selection.BoxSelectionStart.y;

        for (int i = 0; i < clipboard.Rows; i++) {
            int currentRow = row + i;
            
            if (insert)
                InsertRow(currentRow);
            
            for (int j = 0; j < clipboard.Columns; j++) {
                string value = clipboard[i, j];

                if (value != null)
                    SetCellText(currentRow, column + j, value);
            }
        }
        
        EndEdit();
    }

    private void Rename() {
        if (!selection.AnyBoxSelection)
            return;

        var variables = analysis.Cells[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y].VariablesUsed;

        if (variables.Count == 0)
            return;

        var first = variables[0];
        var usages = first.Usages;
        
        textInputPopup.Show($"Enter new name for variable {first.Name}", first.Name, value => Do(usages, value), blocker);

        void Do(List<VariableUsage> usages, string value) {
            if (string.IsNullOrWhiteSpace(value)
                || !Parser.TryParseToken(new StringRange(value), 0, null, false, out var token)
                || token.Type != TokenType.Chain)
                return;

            var chain = (Chain) token;
            
            if (chain.Length != 1 || chain[0].Type != TokenType.Name)
                return;
            
            BeginEdit();
            
            foreach (var usage in usages) {
                var range = analysis.Cells[usage.Row, usage.Column].Tokens[usage.TokenIndex].Range;
                
                SetCellText(usage.Row, usage.Column, range.String.Remove(range.Index, range.Length).Insert(range.Index, value));
            }
            
            EndEdit();
        }
    }

    private bool CanExecuteAction(BindableAction action) => action switch {
        BindableAction.Undo => undoRedo.CanUndo(),
        BindableAction.Redo => undoRedo.CanRedo(),
        BindableAction.Copy => selection.AnySelected,
        BindableAction.Paste => clipboard != null && selection.AnyBoxSelection,
        BindableAction.PasteAndInsert => clipboard != null && selection.AnyBoxSelection,
        BindableAction.Rename => selection.AnyBoxSelection && analysis.Cells[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y].VariablesUsed.Count > 0,
        _ => false
    };

    private bool AnyInRow(int row) {
        for (int i = 0; i < document.Columns; i++) {
            if (!string.IsNullOrWhiteSpace(document[row, i]))
                return true;
        }

        return false;
    }

    private bool AnyInSelectedRows() {
        foreach (int row in selection.GetSelectedRows()) {
            if (AnyInRow(row))
                return true;
        }

        return false;
    }

    private int GetRightmostFilledInRow(int row) {
        for (int i = document.Columns - 1; i >= 0; i--) {
            if (!string.IsNullOrWhiteSpace(document[row, i]))
                return i;
        }

        return -1;
    }

    private static string AutoFormat(StringRange value, bool outermost = true) {
        value = value.Trim();

        int commentIndex = value.IndexOf("//");
        
        if (commentIndex >= 0)
            return value.Substring(commentIndex, value.Length - commentIndex).ToString();

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
}

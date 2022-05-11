using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using StoryboardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private int newDocumentRows;
    [SerializeField] private Color cellColor1;
    [SerializeField] private Color cellColor2;
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private TMP_Dropdown proceduresDropdown;
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
        proceduresDropdown.onValueChanged.AddListener(OnProcedureDropdownItemSelected);
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
        input.Bind(BindableAction.Duplicate, () => Duplicate(false));
        input.Bind(BindableAction.DuplicateAndInsert, () => Duplicate(true));
        input.Bind(BindableAction.Rename, Rename);
        input.Bind(BindableAction.CreateProcedureVariant, CreateProcedureVariant);
        input.Bind(BindableAction.CollapseToProcedure, CollapseToProcedure);
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
        gridView.FocusSelectionStart();
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
        gridView.FocusSelectionStart();
        
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
            gridView.FocusSelectionStart();

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
        gridView.FocusSelectionStart();
        
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
        gridView.FocusSelectionStart();
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
    
    private void OnGridDragEnd(int row, int column, InputModifier modifiers) => UpdateSelection();

    private void OnGridDeselected(InputModifier modifiers) {
        rowSelecting = false;
    }

    private void OnProcedureDropdownItemSelected(int index) {
        if (index > 0)
            index = analysis.Procedures[index - 1].Row;

        selection.SetBoxSelectionStartAndEnd(index, 0);
        UpdateSelection();
        gridView.SetScroll(index);
    }

    #endregion

    #region Logic

    private void SetDocument(Table<string> document) {
        this.document = document;
        StoryboardDocument.Optimize(document);
        cellStates = new Table<CellVisualState>(document.Rows, document.Columns);

        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j] = new CellVisualState(cellColor1);
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
            cellStates.SetValueSafe(row, column, new CellVisualState(value, cellColor1));
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
                cellStates[index, i] = new CellVisualState(cellColor1);
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
                cellStates.SetValueSafe(index, i, new CellVisualState(value, cellColor1));
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
                cellStates[i, j] ??= new CellVisualState(cellColor1);
        }
    }

    private void UpdateContent() {
        for (int i = 0; i < cellStates.Rows; i++) {
            for (int j = 0; j < cellStates.Columns; j++)
                cellStates[i, j].Text = document[i, j];
        }
        
        lock (analysis.Lock) {
            var info = analysis.Cells;
            var procedures = analysis.Procedures;
            int currentProcedureIndex = 0;
            int nextProcedureRow;
            var currentColor = cellColor1;

            if (procedures.Count > 0)
                nextProcedureRow = procedures[0].Row;
            else
                nextProcedureRow = int.MaxValue;

            for (int i = 0; i < cellStates.Rows; i++) {
                bool isProcedureBorder = false;
                
                if (i >= nextProcedureRow) {
                    currentProcedureIndex++;

                    if (currentProcedureIndex < procedures.Count)
                        nextProcedureRow = procedures[currentProcedureIndex].Row;
                    else
                        nextProcedureRow = int.MaxValue;

                    if (currentProcedureIndex % 2 == 0)
                        currentColor = cellColor1;
                    else
                        currentColor = cellColor2;

                    isProcedureBorder = true;
                }
                
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

                    cell.Color = currentColor;
                    cell.IsError = infoCell.IsError;
                    cell.IsProcedureBorder = isProcedureBorder;
                }
            }
            
            proceduresDropdown.ClearOptions();

            var options = new List<string> { "Top" };

            foreach (var procedure in analysis.Procedures)
                options.Add(procedure.Name);
            
            proceduresDropdown.AddOptions(options);
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
            proceduresDropdown.SetValueWithoutNotify(GetProcedureAbove(selection.BoxSelectionStart.x) + 1);
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
        
        selection.ClearSelection();
        selection.SetBoxSelectionStartAndEnd(row + clipboard.Rows, column);
        UpdateSelection();
    }

    private void Duplicate(bool insert) {
        if (!selection.AnySelected)
            return;
        
        BeginEdit();
        
        var bounds = selection.GetSelectionBounds();
        int fromRow = bounds.x;
        int toRow = bounds.xMax;
        int column = bounds.y;

        for (int i = 0; i < bounds.width; i++) {
            int currentFromRow = fromRow + i;
            int currentToRow = toRow + i;
            
            if (insert)
                InsertRow(currentToRow);
            
            for (int j = 0; j < bounds.height; j++) {
                int currentColumn = column + j;

                SetCellText(currentToRow, currentColumn, document[currentFromRow, currentColumn]);
            }
        }
        
        EndEdit();
        
        selection.ClearSelection();
        selection.SetBoxSelectionStart(toRow, column);
        selection.SetBoxSelectionEnd(toRow + bounds.width - 1, bounds.yMax - 1);
        UpdateSelection();
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

        void Do(List<VariableUsage> usages, string newName) {
            if (string.IsNullOrWhiteSpace(newName)
                || !Parser.TryParseToken(new StringRange(newName), 0, null, out var token)
                || token.Type != TokenType.Chain)
                return;

            var chain = (Chain) token;
            
            if (chain.Length != 1 || chain[0].Type != TokenType.Name)
                return;
            
            BeginEdit();
            
            foreach (var usage in usages) {
                var range = analysis.Cells[usage.Row, usage.Column].Tokens[usage.TokenIndex].Range;
                
                SetCellText(usage.Row, usage.Column, range.String.Remove(range.Index, range.Length).Insert(range.Index, newName));
            }
            
            EndEdit();
        }
    }

    private void CreateProcedureVariant() {
        if (!selection.AnyBoxSelection)
            return;

        var variables = analysis.Cells[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y].VariablesUsed;

        if (variables.Count != 1)
            return;

        var procedureInfo = variables[0].ProcedureInfo;
        
        if (procedureInfo == null)
            return;

        string trimmedName = Regex.Match(procedureInfo.Name, @"^(\w*?)(_\d*)?$").Groups[1].Value;
        string defaultName;
        bool nameExists;
        int i = 0;

        do {
            defaultName = $"{trimmedName}_{i}";
            nameExists = false;

            foreach (var procedure in analysis.Procedures) {
                if (defaultName != procedure.Name)
                    continue;

                nameExists = true;

                break;
            }

            i++;
        } while (nameExists);

        textInputPopup.Show($"Enter name for variant of procedure {procedureInfo.Name}", defaultName, value => Do(procedureInfo, value), blocker);

        void Do(ProcedureInfo procedureInfo, string newName) {
            if (string.IsNullOrWhiteSpace(newName)
                || !Parser.TryParseToken(new StringRange(newName), 0, null, out var token)
                || token.Type != TokenType.Chain)
                return;
            
            var chain = (Chain) token;
            
            if (chain.Length != 1 || chain[0].Type != TokenType.Name)
                return;
            
            int startRow = procedureInfo.Row;
            int endRow;
            int procedureIndex = analysis.Procedures.IndexOf(procedureInfo);

            if (procedureIndex == analysis.Procedures.Count - 1)
                endRow = document.Rows;
            else
                endRow = analysis.Procedures[procedureIndex + 1].Row;
            
            BeginEdit();

            var selectedCells = new List<Vector2Int>(selection.GetSelectedCells());

            foreach (var usage in procedureInfo.VariableInfo.Usages) {
                var cell = new Vector2Int(usage.Row, usage.Column);
                
                if (cell == procedureInfo.VariableInfo.Declaration || !selectedCells.Contains(cell))
                    continue;
                
                var range = analysis.Cells[usage.Row, usage.Column].Tokens[usage.TokenIndex].Range;
                
                SetCellText(usage.Row, usage.Column, range.String.Remove(range.Index, range.Length).Insert(range.Index, newName));
            }

            for (int i = startRow, j = endRow; i < endRow; i++, j++) {
                InsertRow(j);
            
                for (int k = 0; k < document.Columns; k++)
                    SetCellText(j, k, document[i, k]);
            }
            
            SetCellText(endRow, 1, newName);
            EndEdit();
            
            selection.ClearSelection();
            selection.ClearBoxSelection();
            UpdateSelection();
        }
    }

    private void CollapseToProcedure() {
        if (!selection.AnyBoxSelection)
            return;
        
        textInputPopup.Show("Enter name for new procedure", "", Do, blocker);

        void Do(string newName) {
            var selectedRows = new List<int>(selection.GetSelectedRows());
            int toRow;
            int procedureIndex = GetProcedureAbove(selectedRows[^1]) + 1;

            if (procedureIndex == analysis.Procedures.Count)
                toRow = document.Rows;
            else
                toRow = analysis.Procedures[procedureIndex].Row;
            
            BeginEdit();
            
            InsertRow(toRow);
            InsertRow(toRow + 1);
            SetCellText(toRow, 0, "proc");
            SetCellText(toRow, 1, newName);
            toRow += 2;

            foreach (int fromRow in selectedRows) {
                InsertRow(toRow);
                
                for (int i = 0; i < document.Columns; i++)
                    SetCellText(toRow, i, document[fromRow, i]);

                toRow++;
            }
            
            InsertRow(toRow);
            InsertRow(toRow + 1);

            for (int i = selectedRows.Count - 1; i >= 1; i--)
                RemoveRow(selectedRows[i]);

            toRow = selectedRows[0];
            
            SetCellText(toRow, 0, "call");
            SetCellText(toRow, 1, "0s");
            SetCellText(toRow, 2, newName);

            for (int i = 3; i < document.Columns; i++)
                SetCellText(toRow, i, string.Empty);
            
            EndEdit();
            
            selection.ClearSelection();
            selection.ClearBoxSelection();
            UpdateSelection();
        }
    }

    private bool CanExecuteAction(BindableAction action) {
        switch (action) {
            case BindableAction.Undo:
                return undoRedo.CanUndo();
            case BindableAction.Redo:
                return undoRedo.CanRedo();
            case BindableAction.Copy:
                return selection.AnySelected;
            case BindableAction.Paste:
            case BindableAction.PasteAndInsert:
                return clipboard != null && selection.AnyBoxSelection;
            case BindableAction.Duplicate:
            case BindableAction.DuplicateAndInsert:
                return selection.AnySelected;
            case BindableAction.Rename:
                return selection.AnyBoxSelection && analysis.Cells[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y].VariablesUsed.Count > 0;
            case BindableAction.CreateProcedureVariant:
                if (!selection.AnyBoxSelection)
                    return false;

                var variables = analysis.Cells[selection.BoxSelectionStart.x, selection.BoxSelectionStart.y].VariablesUsed;

                if (variables.Count != 1)
                    return false;

                var procedureInfo = variables[0].ProcedureInfo;
        
                return procedureInfo != null;
            case BindableAction.CollapseToProcedure:
                return selection.AnySelected;
            default:
                return false;
        }
    }

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

    private int GetProcedureAbove(int row) {
        var procedures = analysis.Procedures;
        int index = selection.BoxSelectionStart.x;

        for (int i = procedures.Count - 1; i >= 0; i--) {
            if (index < procedures[i].Row)
                continue;

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

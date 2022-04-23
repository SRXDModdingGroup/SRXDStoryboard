using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IScrollHandler {
    private class Row {
        public int RowIndex { get; set; }
        
        public List<GridCell> Cells { get; }

        public Row(int rowIndex, List<GridCell> cells) {
            RowIndex = rowIndex;
            Cells = cells;
        }
    }

    private class Column {
        public float Width { get; }
        
        public RectTransform Root { get; }

        public Column(float width, RectTransform root) {
            Width = width;
            Root = root;
        }
    }

    private class CellState {
        public string Text { get; set; }
    }

    [SerializeField] private int initialRowCount;
    [SerializeField] private int initialColumnCount;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform grid;
    [SerializeField] private RectTransform numberColumn;
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject numberCellPrefab;

    public event Action<int, int> OnDragBegin;

    public event Action<int, int> OnDragUpdate;

    public event Action<int, int> OnDragEnd;

    private bool anyBoxSelection;
    private bool mouseDragging;
    private int selectedCount;
    private int scroll;
    private int rowCount;
    private int columnCount;
    private int visibleRowCount;
    private float rowHeight;
    private float numberColumnWidth;
    private Vector2Int boxSelectionStart = new(-1, -1);
    private Vector2Int boxSelectionEnd = new (-1, -1);
    private Vector2Int boxSelectionMin;
    private Vector2Int boxSelectionMax;
    private List<Row> rows;
    private List<Column> columns;
    private List<TMP_Text> numberTexts;
    private List<List<CellState>> cellStates;
    private List<List<bool>> selection;
    private RectTransform rectTransform;

    public void SelectCell(int row, int column) {
        if (!IsInBounds(row, column))
            return;

        SelectCellInternal(row, column);
        UpdateSelection();
    }
    
    public void DeselectCell(int row, int column) {
        if (!IsInBounds(row, column))
            return;

        DeselectCellInternal(row, column);
        UpdateSelection();
    }

    public void SetBoxSelectionStart(int row, int column) {
        boxSelectionStart = ClampToBounds(new Vector2Int(row, column));

        if (!anyBoxSelection)
            boxSelectionEnd = boxSelectionStart;
        
        UpdateSelection();
    }
    
    public void SetBoxSelectionEnd(int row, int column) {
        boxSelectionEnd = ClampToBounds(new Vector2Int(row, column));
        
        if (!anyBoxSelection)
            boxSelectionStart = boxSelectionEnd;
        
        UpdateSelection();
    }

    public void SetRowSelectionStart(int row) {
        if (!IsInBounds(row, 0))
            return;

        boxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        
        if (!anyBoxSelection)
            boxSelectionEnd = boxSelectionStart;
        
        boxSelectionEnd.y = columnCount - 1;
        UpdateSelection();
    }
    
    public void SetRowSelectionEnd(int row) {
        if (!IsInBounds(row, 0))
            return;

        boxSelectionEnd = ClampToBounds(new Vector2Int(row, columnCount - 1));
        
        if (!anyBoxSelection)
            boxSelectionStart = boxSelectionEnd;
        
        boxSelectionStart.y = 0;
        UpdateSelection();
    }

    public void ApplyBoxSelection() {
        var clampedMin = ClampToBounds(boxSelectionMin);
        var clampedMax = ClampToBounds(boxSelectionMax);
                
        for (int i = clampedMin.x; i <= clampedMax.x; i++) {
            for (int j = clampedMin.y; j <= clampedMax.y; j++)
                SelectCellInternal(i, j);
        }
        
        UpdateSelection();
    }

    public void ClearSelection() {
        for (int i = 0; i < rowCount; i++) {
            for (int j = 0; j < columnCount; j++)
                DeselectCellInternal(i, j);
        }
        
        UpdateSelection();
    }

    public void ClearBoxSelection() {
        boxSelectionStart = new Vector2Int(-1, -1);
        boxSelectionEnd = new Vector2Int(-1, -1);
        anyBoxSelection = false;
        UpdateSelection();
    }

    public void SetCellText(int row, int column, string text) {
        if (!IsInBounds(row, column))
            return;

        cellStates[row][column].Text = text;

        var rowAtIndex = GetRow(row);
        
        if (rowAtIndex.RowIndex == row)
            UpdateRowContents(rowAtIndex);
    }

    public void SetScroll(int scroll) {
        scroll = Math.Max(0, Math.Min(scroll, rowCount - visibleRowCount + 2));
        this.scroll = scroll;

        int endRow = scroll + visibleRowCount;

        for (int i = scroll, j = 0; i < endRow; i++, j++) {
            var row = GetRow(i);
            
            if (row.RowIndex != i) {
                SetRowIndex(row, i);
                UpdateRowContents(row);
            }

            foreach (var cell in row.Cells)
                cell.transform.SetSiblingIndex(j);
        }

        for (int i = 0; i < numberTexts.Count; i++)
            numberTexts[i].SetText((scroll + i + 1).ToString());
        
        UpdateSelection();
    }

    public void FocusSelectionStart() {
        if (!anyBoxSelection)
            return;

        int row = boxSelectionStart.x;
        
        if (row < scroll)
            SetScroll(row);
        else if (row >= scroll + visibleRowCount)
            SetScroll(row - visibleRowCount + 1);
    }
    
    public void FocusSelectionEnd() {
        if (!anyBoxSelection)
            return;

        int row = boxSelectionEnd.x;
        
        if (row < scroll)
            SetScroll(row);
        else if (row >= scroll + visibleRowCount)
            SetScroll(row - visibleRowCount + 1);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        mouseDragging = true;

        var index = GetCellIndexAtPosition(eventData.position);

        OnDragBegin?.Invoke(index.x, index.y);
    }
    
    public void OnPointerMove(PointerEventData eventData) {
        if (!mouseDragging)
            return;
        
        var index = GetCellIndexAtPosition(eventData.position);

        OnDragUpdate?.Invoke(index.x, index.y);
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!mouseDragging)
            return;
        
        mouseDragging = false;

        var index = GetCellIndexAtPosition(eventData.position);

        OnDragEnd?.Invoke(index.x, index.y);
    }
    
    public void OnScroll(PointerEventData eventData) {
        if (!eventData.IsScrolling())
            return;
        
        if (eventData.scrollDelta.y > 0f)
            SetScroll(scroll - 1);
        else
            SetScroll(scroll + 1);
    }

    public bool TryGetBoxSelectionStart(out Vector2Int cell) {
        if (anyBoxSelection) {
            cell = boxSelectionStart;

            return true;
        }
        
        cell = Vector2Int.zero;

        return false;
    }
    
    public bool TryGetBoxSelectionEnd(out Vector2Int cell) {
        if (anyBoxSelection) {
            cell = boxSelectionEnd;

            return true;
        }
        
        cell = Vector2Int.zero;

        return false;
    }

    public IEnumerable<Vector2Int> GetSelectedCells() {
        for (int i = 0; i < rowCount; i++) {
            for (int j = 0; j < columnCount; j++) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        visibleRowCount = (int) (viewport.rect.height / 30f) + 1;
        rowCount = initialRowCount;
        rows = new List<Row>(visibleRowCount);
        numberTexts = new List<TMP_Text>(visibleRowCount);

        for (int i = 0; i < visibleRowCount; i++) {
            var row = new List<GridCell>(initialColumnCount);
            
            for (int j = 0; j < initialColumnCount; j++)
                row.Add(null);

            rows.Add(new Row(i, row));

            var numberText = Instantiate(numberCellPrefab, numberColumn).GetComponentInChildren<TMP_Text>();
            
            numberText.SetText((i + 1).ToString());
            numberTexts.Add(numberText);
        }

        float columnWidth = columnPrefab.GetComponent<RectTransform>().rect.width;
        
        columnCount = initialColumnCount;
        columns = new List<Column>(initialColumnCount);

        for (int i = 0; i < initialColumnCount; i++) {
            var column = Instantiate(columnPrefab, grid).GetComponent<RectTransform>();

            for (int j = 0; j < visibleRowCount; j++)
                rows[j].Cells[i] = Instantiate(cellPrefab, column).GetComponent<GridCell>();
            
            columns.Add(new Column(columnWidth, column));
        }

        rowHeight = cellPrefab.GetComponent<RectTransform>().rect.height;
        numberColumnWidth = numberColumn.rect.width;
        cellStates = new List<List<CellState>>(rowCount);
        selection = new List<List<bool>>();

        for (int i = 0; i < rowCount; i++) {
            var row = new List<CellState>(columnCount);
            var rowSelection = new List<bool>(columnCount);

            for (int j = 0; j < columnCount; j++) {
                row.Add(new CellState());
                rowSelection.Add(false);
            }
            
            cellStates.Add(row);
            selection.Add(rowSelection);
        }
        
        SetScroll(0);
    }

    private void SelectCellInternal(int row, int column) {
        if (selection[row][column])
            return;

        selection[row][column] = true;
        selectedCount++;
    }

    private void DeselectCellInternal(int row, int column) {
        if (!selection[row][column])
            return;

        selection[row][column] = false;
        selectedCount--;
    }

    private void UpdateSelection() {
        boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        anyBoxSelection = boxSelectionMin.x < rowCount && boxSelectionMax.x >= 0 && boxSelectionMin.y < columnCount && boxSelectionMax.y >= -1;
        
        foreach (var row in rows) {
            int rowIndex = row.RowIndex;
            var cells = row.Cells;
        
            for (int i = 0; i < cells.Count; i++) {
                if (IsInSelection(rowIndex, i)) {
                    cells[i].SetSelected(true, rowIndex == boxSelectionStart.x && i == boxSelectionStart.y,
                        IsInSelection(rowIndex, i - 1),
                        IsInSelection(rowIndex, i + 1),
                        IsInSelection(rowIndex - 1, i),
                        IsInSelection(rowIndex + 1, i));
                }
                else
                    cells[i].SetSelected(false, false, false, false, false, false);
            }
        }
    }

    private void UpdateRowContents(Row row) {
        int rowIndex = row.RowIndex;
        var cells = row.Cells;

        for (int i = 0; i < columnCount; i++)
            cells[i].SetText(cellStates[rowIndex][i].Text);
    }

    private void SetRowIndex(Row row, int rowIndex) {
        row.RowIndex = rowIndex;

        bool visible = rowIndex < rowCount;

        foreach (var cell in row.Cells)
            cell.gameObject.SetActive(visible);
    }

    private bool IsInBounds(int row, int column) => row >= 0 && row < rowCount && column >= 0 && column < columnCount;

    private bool IsInSelection(int row, int column) =>
        IsInBounds(row, column) 
        && (selectedCount > 0 && selection[row][column] 
            || anyBoxSelection && row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);

    private int GetRowIndexFromMousePosition(float relativeY) => (int) (relativeY / rowHeight) + scroll;

    private Vector2 GetRelativePosition(Vector2 screenPosition) {
        var rectPosition = rectTransform.position;
        
        return new Vector2(screenPosition.x - rectPosition.x, rectPosition.y - screenPosition.y);
    }

    private Vector2Int GetCellIndexAtPosition(Vector2 position) {
        var relativePosition = GetRelativePosition(position);
        int row = GetRowIndexFromMousePosition(relativePosition.y);

        if (row >= rowCount)
            row = rowCount;
        
        float relativeX = relativePosition.x;

        if (relativeX < numberColumnWidth)
            return new Vector2Int(row, -1);

        relativeX -= numberColumnWidth;

        for (int i = 0; i < columnCount; i++) {
            float width = columns[i].Width;

            if (relativeX < width)
                return new Vector2Int(row, i);

            relativeX -= width;
        }
        
        return new Vector2Int(row, columnCount);
    }

    private Vector2Int ClampToBounds(Vector2Int index) => Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(index, new Vector2Int(rowCount - 1, columnCount - 1)));

    private Row GetRow(int index) => rows[index % visibleRowCount];
}

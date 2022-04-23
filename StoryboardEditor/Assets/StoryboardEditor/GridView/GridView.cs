using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public float Width { get; set; }
        
        public RectTransform Root { get; }

        public Column(float width, RectTransform root) {
            Width = width;
            Root = root;
        }
    }

    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform grid;
    [SerializeField] private RectTransform numberColumn;
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject numberCellPrefab;

    public bool Selected => EventSystem.current.currentSelectedGameObject == gameObject;

    public event Action<int, int> DragBegin;

    public event Action<int, int> DragUpdate;

    public event Action<int, int> DragEnd;

    private bool needsUpdate;
    private bool anyBoxSelection;
    private bool mouseDragging;
    private int scroll;
    private int rowCount;
    private int columnCount;
    private int visibleRowCount;
    private float rowHeight;
    private float numberColumnWidth;
    private float defaultColumnWidth;
    private Vector2Int boxSelectionStart = new(-1, -1);
    private Vector2Int boxSelectionEnd = new (-1, -1);
    private Vector2Int boxSelectionMin;
    private Vector2Int boxSelectionMax;
    private List<Row> rows;
    private List<Column> columns;
    private List<TMP_Text> numberTexts;
    private List<List<CellState>> content;
    private RectTransform rectTransform;

    public void UpdateView() => needsUpdate = true;

    public void SetContent(List<List<CellState>> content, int rowCount, int columnCount) {
        this.content = content;
        this.rowCount = rowCount;
        this.columnCount = columnCount;
        needsUpdate = true;
    }

    public void SetBoxSelectionStart(Vector2Int start) {
        boxSelectionStart = start;
        UpdateSelection();
        needsUpdate = true;
    }

    public void SetBoxSelectionEnd(Vector2Int end) {
        boxSelectionEnd = end;
        UpdateSelection();
        needsUpdate = true;
    }
    
    public void SetBoxSelectionStartAndEnd(Vector2Int index) {
        boxSelectionStart = index;
        boxSelectionEnd = index;
        UpdateSelection();
        needsUpdate = true;
    }

    public void SetScroll(int scroll) {
        if (rowCount == 0 || columnCount == 0)
            return;
        
        this.scroll = Math.Max(0, Math.Min(scroll, rowCount - visibleRowCount + 2));
        needsUpdate = true;
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

    public void InsertRow(int index) {
        var row = new List<CellState>();

        for (int i = 0; i < columnCount; i++) {
            row.Add(new CellState());
        }
        
        content.Insert(index, row);
        rowCount++;
        needsUpdate = true;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        mouseDragging = true;

        var index = GetCellIndexAtPosition(eventData.position);

        EventSystem.current.SetSelectedGameObject(gameObject);
        DragBegin?.Invoke(index.x, index.y);
    }
    
    public void OnPointerMove(PointerEventData eventData) {
        if (!mouseDragging)
            return;
        
        var index = GetCellIndexAtPosition(eventData.position);

        DragUpdate?.Invoke(index.x, index.y);
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!mouseDragging)
            return;
        
        mouseDragging = false;

        var index = GetCellIndexAtPosition(eventData.position);

        DragEnd?.Invoke(index.x, index.y);
    }
    
    public void OnScroll(PointerEventData eventData) {
        if (!eventData.IsScrolling())
            return;
        
        if (eventData.scrollDelta.y > 0f)
            SetScroll(scroll - 1);
        else
            SetScroll(scroll + 1);
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        visibleRowCount = (int) (viewport.rect.height / 30f) + 1;
        rows = new List<Row>(visibleRowCount);
        numberTexts = new List<TMP_Text>(visibleRowCount);

        for (int i = 0; i < visibleRowCount; i++) {
            rows.Add(new Row(i, new List<GridCell>()));

            var numberText = Instantiate(numberCellPrefab, numberColumn).GetComponentInChildren<TMP_Text>();
            
            numberText.SetText((i + 1).ToString());
            numberTexts.Add(numberText);
        }

        columns = new List<Column>();
        rowHeight = cellPrefab.GetComponent<RectTransform>().rect.height;
        numberColumnWidth = numberColumn.rect.width;
        defaultColumnWidth = columnPrefab.GetComponent<RectTransform>().rect.width;
        SetScroll(0);
    }

    private void Update() {
        if (!needsUpdate)
            return;
        
        if (content == null || content.Count == 0) {
            gameObject.SetActive(false);
            
            return;
        }
        
        gameObject.SetActive(true);

        for (int i = 0; i < numberTexts.Count; i++)
            numberTexts[i].SetText((scroll + i + 1).ToString());

        while (columns.Count < columnCount)
            columns.Add(new Column(defaultColumnWidth, Instantiate(columnPrefab, grid).GetComponent<RectTransform>()));

        for (int i = scroll, j = 0; j < visibleRowCount; i++, j++) {
            var row = rows[i % visibleRowCount];
            var cells = row.Cells;

            row.RowIndex = i;
            
            while (cells.Count < columnCount)
                cells.Add(Instantiate(cellPrefab, columns[cells.Count].Root).GetComponent<GridCell>());

            for (int k = 0; k < columnCount; k++) {
                var cell = cells[k];
                
                cell.transform.SetSiblingIndex(j);
                
                if (!IsInBounds(i, k)) {
                    cell.gameObject.SetActive(false);
                    
                    continue;
                }
                
                cell.gameObject.SetActive(true);
                cell.SetText(content[i][k].DisplayedText);
            
                if (IsInSelection(i, k)) {
                    cell.SetSelected(true, i == boxSelectionStart.x && k == boxSelectionStart.y,
                        IsInSelection(i, k - 1),
                        IsInSelection(i, k + 1),
                        IsInSelection(i - 1, k),
                        IsInSelection(i + 1, k));
                }
                else
                    cell.SetSelected(false, false, false, false, false, false);
                
                if (IsInBounds(i, k))
                    cell.SetText(content[i][k].DisplayedText);

                bool IsInSelection(int row, int column) =>
                    IsInBounds(row, column) 
                    && (content[row][column].Selected
                        || anyBoxSelection && row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
            }
        }

        needsUpdate = false;
    }

    private void UpdateSelection() {
        boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        anyBoxSelection = boxSelectionMin.x < rowCount && boxSelectionMax.x >= 0 && boxSelectionMin.y < columnCount && boxSelectionMax.y >= -1;
    }

    private bool IsInBounds(int row, int column) => row >= 0 && row < rowCount && column >= 0 && column < columnCount;

    private int GetRowIndexFromMousePosition(float relativeY) => Math.Clamp((int) (relativeY / rowHeight) + scroll, 0, rowCount - 1);

    private Vector2Int GetCellIndexAtPosition(Vector2 position) {
        var relativePosition = GetRelativePosition(position, rectTransform);
        float relativeX = relativePosition.x;
        int row = GetRowIndexFromMousePosition(relativePosition.y);

        if (relativeX < numberColumnWidth)
            return new Vector2Int(row, -1);

        relativeX = GetRelativePosition(position, grid).x;

        for (int i = 0; i < columnCount; i++) {
            float width = columns[i].Width;

            if (relativeX < width)
                return new Vector2Int(row, i);

            relativeX -= width;
        }
        
        return new Vector2Int(row, columnCount);
    }

    private static Vector2 GetRelativePosition(Vector2 screenPosition, RectTransform relativeTo) {
        var rectPosition = relativeTo.position;
        
        return new Vector2(screenPosition.x - rectPosition.x, rectPosition.y - screenPosition.y);
    }
}

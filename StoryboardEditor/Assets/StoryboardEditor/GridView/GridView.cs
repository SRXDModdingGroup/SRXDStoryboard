using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IScrollHandler, IDeselectHandler {
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

    [SerializeField] private float scrollSpacePerRow;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform grid;
    [SerializeField] private RectTransform numberColumn;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject numberCellPrefab;

    public event Action<int, int> DragBegin;

    public event Action<int, int> DragUpdate;

    public event Action<int, int> DragEnd;

    public event Action Deselected;

    private bool viewNeedsUpdate;
    private bool anyBoxSelection;
    private bool mouseDragging;
    private int scroll;
    private int maxScroll;
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
    private Table<CellVisualState> cellStates;
    private RectTransform rectTransform;

    public void UpdateView() => viewNeedsUpdate = true;

    public void SetCellStates(Table<CellVisualState> cellStates) {
        this.cellStates = cellStates;
        viewNeedsUpdate = true;
    }

    public void SetBoxSelectionStart(Vector2Int start) {
        boxSelectionStart = start;
        UpdateSelection();
        viewNeedsUpdate = true;
    }

    public void SetBoxSelectionEnd(Vector2Int end) {
        boxSelectionEnd = end;
        UpdateSelection();
        viewNeedsUpdate = true;
    }
    
    public void SetBoxSelectionStartAndEnd(Vector2Int index) {
        boxSelectionStart = index;
        boxSelectionEnd = index;
        UpdateSelection();
        viewNeedsUpdate = true;
    }

    public void SetScroll(int scroll) {
        if (cellStates == null || cellStates.Empty)
            return;

        maxScroll = Math.Max(0, cellStates.Rows - visibleRowCount + 1);
        this.scroll = Math.Max(0, Math.Min(scroll, maxScroll));
        viewNeedsUpdate = true;
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

        var eventSystem = EventSystem.current;
        
        if (eventSystem == null || eventSystem.alreadySelecting)
            return;
        
        mouseDragging = true;

        var index = GetCellIndexAtPosition(eventData.position);

        EventSystem.current.SetSelectedGameObject(gameObject);
        DragBegin?.Invoke(index.x, index.y);
    }
    
    public void OnPointerMove(PointerEventData eventData) {
        if (!mouseDragging || EventSystem.current.currentSelectedGameObject != gameObject)
            return;
        
        var index = GetCellIndexAtPosition(eventData.position);

        DragUpdate?.Invoke(index.x, index.y);
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!mouseDragging || EventSystem.current.currentSelectedGameObject != gameObject)
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

    public void OnDeselect(BaseEventData eventData) {
        mouseDragging = false;
        Deselected?.Invoke();
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        rowHeight = cellPrefab.GetComponent<RectTransform>().rect.height;
        visibleRowCount = (int) (viewport.rect.height / rowHeight) + 1;
        rows = new List<Row>(visibleRowCount);
        numberTexts = new List<TMP_Text>(visibleRowCount);

        for (int i = 0; i < visibleRowCount; i++) {
            rows.Add(new Row(i, new List<GridCell>()));

            var numberText = Instantiate(numberCellPrefab, numberColumn).GetComponentInChildren<TMP_Text>();
            
            numberText.SetText((i + 1).ToString());
            numberTexts.Add(numberText);
        }

        columns = new List<Column>();
        numberColumnWidth = numberColumn.rect.width;
        defaultColumnWidth = columnPrefab.GetComponent<RectTransform>().rect.width;
        scrollbar.onValueChanged.AddListener(value => SetScroll(Mathf.FloorToInt(value * maxScroll)));
    }

    private void LateUpdate() {
        if (!viewNeedsUpdate)
            return;
        
        if (cellStates == null || cellStates.Rows == 0 || cellStates.Columns == 0) {
            gameObject.SetActive(false);
            
            return;
        }
        
        gameObject.SetActive(true);

        for (int i = 0; i < numberTexts.Count; i++)
            numberTexts[i].SetText((scroll + i + 1).ToString());

        while (columns.Count < cellStates.Columns)
            columns.Add(new Column(defaultColumnWidth, Instantiate(columnPrefab, grid).GetComponent<RectTransform>()));

        for (int i = scroll, j = 0; j < visibleRowCount; i++, j++) {
            var row = rows[i % visibleRowCount];
            var cells = row.Cells;

            row.RowIndex = i;
            
            while (cells.Count < columns.Count)
                cells.Add(Instantiate(cellPrefab, columns[cells.Count].Root).GetComponent<GridCell>());

            for (int k = 0; k < columns.Count; k++) {
                var cell = cells[k];
                
                cell.transform.SetSiblingIndex(j);
                
                if (!IsInBounds(i, k)) {
                    cell.gameObject.SetActive(false);
                    
                    continue;
                }
                
                cell.gameObject.SetActive(true);
                cell.SetText(cellStates[i, k].Text);
            
                if (IsInSelection(i, k)) {
                    cell.SetSelected(true, anyBoxSelection && i == boxSelectionStart.x && k == boxSelectionStart.y,
                        IsInSelection(i, k - 1),
                        IsInSelection(i, k + 1),
                        IsInSelection(i - 1, k),
                        IsInSelection(i + 1, k));
                }
                else
                    cell.SetSelected(false, false, false, false, false, false);
                
                if (IsInBounds(i, k))
                    cell.SetText(cellStates[i, k].Text);

                bool IsInSelection(int row, int column) =>
                    IsInBounds(row, column) 
                    && (cellStates[row, column].Selected
                        || anyBoxSelection && row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
            }
        }
        
        maxScroll = Math.Max(0, cellStates.Rows - visibleRowCount + 1);

        if (maxScroll == 0) {
            scrollbar.SetValueWithoutNotify(0f);
            scrollbar.size = 1f;
        }
        else {
            scrollbar.SetValueWithoutNotify((float) scroll / maxScroll);
            scrollbar.size = Mathf.Clamp01(1f - maxScroll * scrollSpacePerRow);
        }
        
        viewNeedsUpdate = false;
    }

    private void UpdateSelection() {
        boxSelectionMin = Vector2Int.Min(boxSelectionStart, boxSelectionEnd);
        boxSelectionMax = Vector2Int.Max(boxSelectionStart, boxSelectionEnd);
        anyBoxSelection = boxSelectionMin.x < cellStates.Rows && boxSelectionMax.x >= 0 && boxSelectionMin.y < cellStates.Columns && boxSelectionMax.y >= -1;
    }

    private bool IsInBounds(int row, int column) => row >= 0 && row < cellStates.Rows && column >= 0 && column < cellStates.Columns;

    private int GetRowIndexFromMousePosition(float relativeY) => Math.Clamp((int) (relativeY / rowHeight) + scroll, 0, cellStates.Rows - 1);

    private Vector2Int GetCellIndexAtPosition(Vector2 position) {
        var relativePosition = GetRelativePosition(position, rectTransform);
        float relativeX = relativePosition.x;
        int row = GetRowIndexFromMousePosition(relativePosition.y);

        if (relativeX < numberColumnWidth)
            return new Vector2Int(row, -1);

        relativeX = GetRelativePosition(position, grid).x;

        for (int i = 0; i < cellStates.Columns; i++) {
            float width = columns[i].Width;

            if (relativeX < width)
                return new Vector2Int(row, i);

            relativeX -= width;
        }
        
        return new Vector2Int(row, cellStates.Columns);
    }

    private static Vector2 GetRelativePosition(Vector2 screenPosition, RectTransform relativeTo) {
        var rectPosition = relativeTo.position;
        
        return new Vector2(screenPosition.x - rectPosition.x, rectPosition.y - screenPosition.y);
    }
}

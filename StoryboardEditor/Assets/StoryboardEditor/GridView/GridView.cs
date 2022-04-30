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

    public event Action<int, int, EditorInput.InputModifier> DragBegin;

    public event Action<int, int, EditorInput.InputModifier> DragUpdate;

    public event Action<int, int, EditorInput.InputModifier> DragEnd;

    public event Action<EditorInput.InputModifier> Deselected;

    private bool viewNeedsUpdate;
    private bool mouseDragging;
    private int scroll;
    private int maxScroll;
    private int visibleRowCount;
    private float rowHeight;
    private float numberColumnWidth;
    private float defaultColumnWidth;
    private List<Row> rows;
    private List<Column> columns;
    private List<TMP_Text> numberTexts;
    private Table<CellVisualState> cellStates;
    private EditorSelection selection;
    private RectTransform rectTransform;

    public void UpdateView() => viewNeedsUpdate = true;

    public void Init(Table<CellVisualState> cellStates, EditorSelection selection) {
        this.cellStates = cellStates;
        this.selection = selection;
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
        if (!selection.AnyBoxSelection)
            return;

        int row = selection.BoxSelectionStart.x;
        
        if (row < scroll)
            SetScroll(row);
        else if (row >= scroll + visibleRowCount)
            SetScroll(row - visibleRowCount + 1);
    }
    
    public void FocusSelectionEnd() {
        if (!selection.AnyBoxSelection)
            return;

        int row = selection.BoxSelectionEnd.x;
        
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
        DragBegin?.Invoke(index.x, index.y, EditorInput.GetModifiers());
    }
    
    public void OnPointerMove(PointerEventData eventData) {
        if (!mouseDragging || EventSystem.current.currentSelectedGameObject != gameObject)
            return;
        
        var index = GetCellIndexAtPosition(eventData.position);

        DragUpdate?.Invoke(index.x, index.y, EditorInput.GetModifiers());
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!mouseDragging || EventSystem.current.currentSelectedGameObject != gameObject)
            return;
        
        mouseDragging = false;

        var index = GetCellIndexAtPosition(eventData.position);

        DragEnd?.Invoke(index.x, index.y, EditorInput.GetModifiers());
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
        Deselected?.Invoke(EditorInput.GetModifiers());
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
        maxScroll = Math.Max(0, cellStates.Rows - visibleRowCount + 1);
        scroll = Math.Max(0, Math.Min(scroll, maxScroll));
        
        if (maxScroll == 0) {
            scrollbar.SetValueWithoutNotify(0f);
            scrollbar.size = 1f;
        }
        else {
            scrollbar.SetValueWithoutNotify((float) scroll / maxScroll);
            scrollbar.size = Mathf.Clamp01(1f - maxScroll * scrollSpacePerRow);
        }

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

                var cellState = cellStates[i, k];
                
                cell.gameObject.SetActive(true);
                cell.SetText(cellState.FormattedText);
            
                if (selection.IsInSelection(i, k)) {
                    cell.SetSelected(true, selection.AnyBoxSelection && i == selection.BoxSelectionStart.x && k == selection.BoxSelectionStart.y,
                        selection.IsInSelection(i, k - 1),
                        selection.IsInSelection(i, k + 1),
                        selection.IsInSelection(i - 1, k),
                        selection.IsInSelection(i + 1, k));
                }
                else
                    cell.SetSelected(false, false, false, false, false, false);
                
                cell.SetIsError(cellState.IsError);
            }
        }

        viewNeedsUpdate = false;
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

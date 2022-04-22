using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IScrollHandler {
    private class Row {
        public int RowIndex { get; private set; }
        
        public List<GridCell> Cells { get; }

        public Row(int rowIndex, List<GridCell> cells) {
            RowIndex = rowIndex;
            Cells = cells;
        }

        public void UpdateContent() {
            
        }

        public void UpdateSelected(Vector2Int min, Vector2Int max) {
            
        }

        public void SetRowIndex(int rowIndex, int rowCount) {
            RowIndex = rowIndex;

            bool visible = rowIndex < rowCount;

            foreach (var cell in Cells)
                cell.gameObject.SetActive(visible);
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
        
    }
    
    [SerializeField] private int initialColumnCount;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform grid;
    [SerializeField] private RectTransform numberColumn;
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject numberCellPrefab;

    private int scroll;
    private int rowCount;
    private int columnCount;
    private int visibleRowCount;
    private float rowHeight;
    private float numberColumnWidth;
    private bool anySelected;
    private bool selecting;
    private bool selectingRows;
    private Vector2Int selectionStart;
    private Vector2Int selectionEnd;
    private List<Row> rows;
    private List<Column> columns;
    private List<TMP_Text> numberTexts;
    private RectTransform rectTransform;

    public void SetScroll(int scroll) {
        scroll = Math.Max(0, Math.Min(scroll, rowCount - visibleRowCount + 2));

        this.scroll = scroll;

        int endRow = scroll + visibleRowCount;

        for (int i = scroll, j = 0; i < endRow; i++, j++) {
            var row = GetRow(i);
            
            if (row.RowIndex != i)
                row.SetRowIndex(i, rowCount);

            foreach (var cell in row.Cells)
                cell.transform.SetSiblingIndex(j);
        }

        for (int i = 0; i < numberTexts.Count; i++)
            numberTexts[i].SetText((scroll + i + 1).ToString());
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        ClearSelection();
        selectionStart = GetCellIndexAtPosition(eventData.position);

        if (selectionStart.y == -1) {
            selectingRows = true;
            selectionEnd = new Vector2Int(selectionStart.x, columnCount - 1);
        }
        else {
            selectingRows = false;
            selectionEnd = selectionStart;
        }
        
        selecting = true;
        UpdateSelection();
    }
    
    public void OnPointerMove(PointerEventData eventData) {
        if (!selecting)
            return;
        
        var index = GetCellIndexAtPosition(eventData.position);

        if (selectingRows)
            selectionEnd = new Vector2Int(index.x, columnCount - 1);
        else
            selectionEnd = index;
        
        UpdateSelection();
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!selecting)
            return;
        
        UpdateSelection();
        selecting = false;
        selectingRows = false;
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
        visibleRowCount = (int) (viewport.rect.height / 30f) + 2;
        rowCount = visibleRowCount;
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
    }

    private void ClearSelection() {
        
    }

    private void UpdateSelection() {
        var min = Vector2Int.Min(selectionStart, selectionEnd);
        var max = Vector2Int.Max(selectionStart, selectionEnd);
        
        foreach (var row in rows)
            row.UpdateSelected(min, max);

        anySelected = min.x < rowCount && max.x >= 0 && min.y < columnCount && max.y >= -1;
    }

    private int GetRowIndexFromMousePosition(float relativeY) => (int) (relativeY / rowHeight) + scroll;

    private Vector2 GetRelativePosition(Vector2 screenPosition) {
        var rectPosition = rectTransform.position;
        
        return new Vector2(screenPosition.x - rectPosition.x, rectPosition.y - screenPosition.y);
    }

    private Vector2Int GetCellIndexAtPosition(Vector2 position) {
        var relativePosition = GetRelativePosition(position);
        int row = GetRowIndexFromMousePosition(relativePosition.y);

        if (row >= rowCount)
            row = -1;
        
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

    private Row GetRow(int index) => rows[index % visibleRowCount];
}

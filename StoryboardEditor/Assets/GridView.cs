using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler {
    private class Row {
        public int RowIndex { get; private set; }
        
        public List<GridCell> Cells { get; }

        public Row(int rowIndex, List<GridCell> cells) {
            RowIndex = rowIndex;
            Cells = cells;
        }

        public void SetContent(int rowIndex) {
            RowIndex = rowIndex;
        }
    }
    
    [SerializeField] private int initialColumnCount;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform grid;
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private GameObject cellPrefab;

    private int scroll;
    private int rowCount;
    private List<Row> rows;
    private List<RectTransform> columns;

    public void SetScroll(int scroll) {
        if (scroll < 0)
            scroll = 0;

        this.scroll = scroll;

        int endRow = scroll + rowCount;

        for (int i = scroll, j = 0; i < endRow; i++, j++) {
            var row = GetRow(i);
            
            if (row.RowIndex != i)
                row.SetContent(i);

            foreach (var cell in row.Cells)
                cell.transform.SetSiblingIndex(j);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("Down");
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        Debug.Log("Up");
    }
    
    public void OnScroll(PointerEventData eventData) {
        if (!eventData.IsScrolling())
            return;
        
        if (eventData.scrollDelta.y > 0f)
            SetScroll(scroll + 1);
        else
            SetScroll(scroll - 1);
    }
    
    private void Awake() {
        rowCount = (int) (viewport.rect.height / 30f) + 2;

        
        
        rows = new List<Row>(rowCount);

        for (int i = 0; i < rowCount; i++) {
            var row = new List<GridCell>(initialColumnCount);
            
            for (int j = 0; j < initialColumnCount; j++)
                row.Add(null);

            rows.Add(new Row(i, row));
        }

        columns = new List<RectTransform>(initialColumnCount);

        for (int i = 0; i < initialColumnCount; i++) {
            var column = Instantiate(columnPrefab, grid).GetComponent<RectTransform>();

            for (int j = 0; j < rowCount; j++)
                rows[j].Cells[i] = Instantiate(cellPrefab, column).GetComponent<GridCell>();
            
            columns.Add(column);
        }
    }

    private Row GetRow(int index) => rows[index % rowCount];
}

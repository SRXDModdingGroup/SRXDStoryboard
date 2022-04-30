using System.Collections.Generic;
using UnityEngine;

public class EditorSelection {
    public bool AnySelected { get; private set; }
    
    public bool AnyBoxSelection { get; private set; }
    
    public Vector2Int BoxSelectionStart { get; private set; } = new(-1, -1);
    
    public Vector2Int BoxSelectionEnd { get; private set; } = new (-1, -1);
    
    private bool selectionNeedsUpdate;
    private Table<bool> selection = new();

    public void SetSize(int rows, int columns) {
        selection.SetSize(rows, columns);
        BoxSelectionStart = ClampToBounds(BoxSelectionStart);
        BoxSelectionEnd = ClampToBounds(BoxSelectionEnd);
    }

    public void UpdateSelection() {
        AnyBoxSelection = IsInBounds(BoxSelectionStart.x, BoxSelectionStart.y);
        
        if (AnyBoxSelection) {
            BoxSelectionStart = ClampToBounds(BoxSelectionStart);
            BoxSelectionEnd = ClampToBounds(BoxSelectionEnd);
            AnySelected = true;
        }
        else {
            BoxSelectionStart = new Vector2Int(-1, -1);
            BoxSelectionEnd = BoxSelectionStart;
            AnySelected = false;
            
            for (int i = 0; i < selection.Rows; i++) {
                for (int j = 0; j < selection.Columns; j++) {
                    if (!IsInSelection(i, j))
                        continue;
                    
                    AnySelected = true;
                        
                    break;
                }

                if (AnySelected)
                    break;
            }
        }
    }

    public void Select(int row, int column) {
        if (IsInBounds(row, column))
            selection[row, column] = true;
    }
    
    public void SetBoxSelectionStart(int row, int column) => BoxSelectionStart = ClampToBounds(new Vector2Int(row, column));

    public void SetBoxSelectionEnd(int row, int column) => BoxSelectionEnd = ClampToBounds(new Vector2Int(row, column));

    public void SetBoxSelectionStartAndEnd(int row, int column) {
        BoxSelectionStart = ClampToBounds(new Vector2Int(row, column));
        BoxSelectionEnd = BoxSelectionStart;
    }
    
    public void SetRowSelectionStart(int row) {
        BoxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        BoxSelectionEnd = new Vector2Int(BoxSelectionEnd.x, selection.Columns - 1);
    }
    
    public void SetRowSelectionEnd(int row) {
        BoxSelectionEnd = ClampToBounds(new Vector2Int(row, selection.Columns - 1));
        BoxSelectionStart = new Vector2Int(BoxSelectionStart.x, 0);
    }

    public void SetRowSelectionStartAndEnd(int row) {
        BoxSelectionStart = ClampToBounds(new Vector2Int(row, 0));
        BoxSelectionEnd = new Vector2Int(BoxSelectionStart.x, selection.Columns - 1);
    }

    public void ApplyBoxSelection() {
        var clampedMin = ClampToBounds(Vector2Int.Min(BoxSelectionStart, BoxSelectionEnd));
        var clampedMax = ClampToBounds(Vector2Int.Max(BoxSelectionStart, BoxSelectionEnd));
                
        for (int i = clampedMin.x; i <= clampedMax.x; i++) {
            for (int j = clampedMin.y; j <= clampedMax.y; j++)
                selection[i, j] = true;
        }
    }

    public void ClearSelection() {
        for (int i = 0; i < selection.Rows; i++) {
            for (int j = 0; j < selection.Columns; j++)
                selection[i, j] = false;
        }
    }

    public void ClearBoxSelection() {
        BoxSelectionStart = new Vector2Int(-1, -1);
        BoxSelectionEnd = BoxSelectionStart;
    }
    
    public bool IsInSelection(int row, int column) {
        var boxSelectionMin = Vector2Int.Min(BoxSelectionStart, BoxSelectionEnd);
        var boxSelectionMax = Vector2Int.Max(BoxSelectionStart, BoxSelectionEnd);
        
        return IsInBounds(row, column) && (selection[row, column] || row >= boxSelectionMin.x && row <= boxSelectionMax.x && column >= boxSelectionMin.y && column <= boxSelectionMax.y);
    }

    public int GetRightmostSelectedInRow(int row) {
        int rightmost = -1;
            
        for (int j = 0; j < selection.Columns; j++) {
            if (IsInSelection(row, j))
                rightmost = j;
        }

        if (rightmost >= 0)
            return rightmost;

        return -1;
    }

    public int GetLeftmostSelectedInRow(int row) {
        int leftmost = -1;
            
        for (int j = 0; j < selection.Columns; j++) {
            if (!IsInSelection(row, j))
                continue;
            
            leftmost = j;

            break;
        }

        if (leftmost >= 0)
            return leftmost;

        return -1;
    }
    
    public int GetTopOfSelection() {
        for (int i = 0; i < selection.Rows; i++) {
            for (int j = 0; j < selection.Columns; j++) {
                if (IsInSelection(i, j))
                    return i;
            }
        }

        return 0;
    }

    public int GetBottomOfSelection() {
        for (int i = selection.Rows - 1; i >= 0; i--) {
            for (int j = 0; j < selection.Columns; j++) {
                if (IsInSelection(i, j))
                    return i;
            }
        }

        return 0;
    }
    
    public IEnumerable<int> GetSelectedRows() {
        for (int i = 0; i < selection.Rows; i++) {
            for (int j = 0; j < selection.Columns; j++) {
                if (!IsInSelection(i, j))
                    continue;
                
                yield return i;

                break;
            }
        }
    }

    public IEnumerable<int> GetSelectedRowsReversed() {
        for (int i = selection.Rows - 1; i >= 0; i--) {
            for (int j = 0; j < selection.Columns; j++) {
                if (!IsInSelection(i, j))
                    continue;
                
                yield return i;

                break;
            }
        }
    }

    public IEnumerable<Vector2Int> GetSelectedCells() {
        for (int i = 0; i < selection.Rows; i++) {
            for (int j = 0; j < selection.Columns; j++) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    public IEnumerable<Vector2Int> GetSelectedCellsReversed() {
        for (int i = 0; i < selection.Rows; i++) {
            for (int j = selection.Columns - 1; j >= 0; j--) {
                if (IsInSelection(i, j))
                    yield return new Vector2Int(i, j);
            }
        }
    }

    public IEnumerable<Vector2Int> GetRightmostSelectedPerRow() {
        for (int i = 0; i < selection.Rows; i++) {
            int rightmost = GetRightmostSelectedInRow(i);

            if (IsInBounds(i, rightmost))
                yield return new Vector2Int(i, rightmost);
        }
    }

    public IEnumerable<Vector2Int> GetLeftmostSelectedPerRow() {
        for (int i = 0; i < selection.Rows; i++) {
            int leftmost = GetLeftmostSelectedInRow(i);

            if (IsInBounds(i, leftmost))
                yield return new Vector2Int(i, leftmost);
        }
    }

    private bool IsInBounds(int row, int column) => row >= 0 && row < selection.Rows && column >= 0 && column < selection.Columns;

    private Vector2Int ClampToBounds(Vector2Int index)
        => Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(index, new Vector2Int(selection.Rows - 1, selection.Columns - 1)));
}
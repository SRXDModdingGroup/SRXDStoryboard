using System;

public class Table<T> {
    private const int DEFAULT_CAPACITY = 4;
    
    public int RowCount { get; private set; }
    
    public int ColumnCount { get; private set; }

    public bool Empty => RowCount == 0 || ColumnCount == 0;
    
    private T[,] data;
    private int rowCapacity;
    private int columnCapacity;

    public Table() {
        data = new T[DEFAULT_CAPACITY, DEFAULT_CAPACITY];
        rowCapacity = DEFAULT_CAPACITY;
        columnCapacity = DEFAULT_CAPACITY;
    }
    
    public Table(int rowCount, int columnCount) {
        rowCapacity = DEFAULT_CAPACITY;
        columnCapacity = DEFAULT_CAPACITY;
        
        while (rowCapacity < rowCount)
            rowCapacity *= 2;

        while (columnCapacity < columnCount)
            columnCapacity *= 2;

        data = new T[rowCapacity, columnCapacity];
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public T this[int row, int column] {
        get {
            if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
                throw new IndexOutOfRangeException();

            return data[row, column];
        }
        set {
            if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
                throw new IndexOutOfRangeException();

            data[row, column] = value;
        }
    }

    public void AddRow() {
        EnsureCapacity(RowCount + 1, ColumnCount);
        RowCount++;
    }

    public void AddColumn() {
        EnsureCapacity(RowCount, ColumnCount + 1);
        ColumnCount++;
    }

    public void InsertRow(int index) {
        if (index == RowCount) {
            AddRow();
            
            return;
        }

        if (index < 0 || index > RowCount)
            throw new IndexOutOfRangeException();
        
        EnsureCapacity(RowCount + 1, ColumnCount);
        RowCount++;

        for (int i = RowCount - 1; i > index; i--) {
            for (int j = 0; j < ColumnCount; j++)
                data[i, j] = data[i - 1, j];
        }

        for (int i = 0; i < ColumnCount; i++)
            data[index, i] = default;
    }

    public void InsertColumn(int index) {
        if (index == ColumnCount) {
            AddColumn();
            
            return;
        }
        
        if (index < 0 || index > ColumnCount)
            throw new IndexOutOfRangeException();
        
        EnsureCapacity(RowCount, ColumnCount + 1);
        ColumnCount++;

        for (int i = 0; i < RowCount; i++) {
            for (int j = ColumnCount - 1; j > index; j--)
                data[i, j] = data[i, j - 1];
        }

        for (int i = 0; i < RowCount; i++)
            data[i, index] = default;
    }

    public void RemoveLastRow() {
        if (RowCount == 0)
            throw new ArgumentOutOfRangeException();
        
        RowCount--;

        for (int i = 0; i < ColumnCount; i++)
            data[RowCount, i] = default;
    }

    public void RemoveLastColumn() {
        if (ColumnCount == 0)
            throw new ArgumentOutOfRangeException();

        ColumnCount--;

        for (int i = 0; i < RowCount; i++)
            data[i, ColumnCount] = default;
    }

    public void RemoveRow(int index) {
        if (index == RowCount - 1) {
            RemoveLastRow();
            
            return;
        }

        if (index < 0 || index >= RowCount)
            throw new IndexOutOfRangeException();
        
        RowCount--;

        for (int i = index; i < RowCount; i++) {
            for (int j = 0; j < ColumnCount; j++)
                data[index, j] = data[index + 1, j];
        }

        for (int i = 0; i < ColumnCount; i++)
            data[RowCount, i] = default;
    }

    public void RemoveColumn(int index) {
        if (index == ColumnCount - 1) {
            RemoveLastColumn();
            
            return;
        }

        if (index < 0 || index >= ColumnCount)
            throw new IndexOutOfRangeException();

        ColumnCount--;

        for (int i = 0; i < RowCount; i++) {
            for (int j = index; j < ColumnCount; j++)
                data[i, index] = data[i, index + 1];
        }

        for (int i = 0; i < RowCount; i++)
            data[i, ColumnCount] = default;
    }

    private void EnsureCapacity(int row, int column) {
        if (rowCapacity >= row && columnCapacity >= column)
            return;

        while (rowCapacity < row)
            rowCapacity *= 2;

        while (columnCapacity < column)
            columnCapacity *= 2;

        var newData = new T[rowCapacity, columnCapacity];

        for (int i = 0; i < RowCount; i++) {
            for (int j = 0; j < ColumnCount; j++)
                newData[i, j] = data[i, j];
        }

        data = newData;
    }
}

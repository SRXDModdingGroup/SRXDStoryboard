using System;

public class Table<T> {
    private const int DEFAULT_CAPACITY = 4;
    
    public int Rows { get; private set; }
    
    public int Columns { get; private set; }

    public bool Empty => Rows == 0 || Columns == 0;
    
    private T[,] data;
    private int rowCapacity;
    private int columnCapacity;

    public Table() {
        data = new T[DEFAULT_CAPACITY, DEFAULT_CAPACITY];
        rowCapacity = DEFAULT_CAPACITY;
        columnCapacity = DEFAULT_CAPACITY;
    }
    
    public Table(int rows, int columns) {
        rowCapacity = DEFAULT_CAPACITY;
        columnCapacity = DEFAULT_CAPACITY;
        
        while (rowCapacity < rows)
            rowCapacity *= 2;

        while (columnCapacity < columns)
            columnCapacity *= 2;

        data = new T[rowCapacity, columnCapacity];
        Rows = rows;
        Columns = columns;
    }

    public T this[int row, int column] {
        get {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                throw new IndexOutOfRangeException();

            return data[row, column];
        }
        set {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                throw new IndexOutOfRangeException();

            data[row, column] = value;
        }
    }

    public void AddRow() {
        EnsureCapacity(Rows + 1, Columns);
        Rows++;
    }

    public void AddColumn() {
        EnsureCapacity(Rows, Columns + 1);
        Columns++;
    }

    public void InsertRow(int index) {
        if (index == Rows) {
            AddRow();
            
            return;
        }

        if (index < 0 || index > Rows)
            throw new IndexOutOfRangeException();
        
        EnsureCapacity(Rows + 1, Columns);
        Rows++;

        for (int i = Rows - 1; i > index; i--) {
            for (int j = 0; j < Columns; j++)
                data[i, j] = data[i - 1, j];
        }

        for (int i = 0; i < Columns; i++)
            data[index, i] = default;
    }

    public void InsertColumn(int index) {
        if (index == Columns) {
            AddColumn();
            
            return;
        }
        
        if (index < 0 || index > Columns)
            throw new IndexOutOfRangeException();
        
        EnsureCapacity(Rows, Columns + 1);
        Columns++;

        for (int i = 0; i < Rows; i++) {
            for (int j = Columns - 1; j > index; j--)
                data[i, j] = data[i, j - 1];
        }

        for (int i = 0; i < Rows; i++)
            data[i, index] = default;
    }

    public void RemoveLastRow() {
        if (Rows == 0)
            throw new ArgumentOutOfRangeException();
        
        Rows--;

        for (int i = 0; i < Columns; i++)
            data[Rows, i] = default;
    }

    public void RemoveLastColumn() {
        if (Columns == 0)
            throw new ArgumentOutOfRangeException();

        Columns--;

        for (int i = 0; i < Rows; i++)
            data[i, Columns] = default;
    }

    public void RemoveRow(int index) {
        if (index == Rows - 1) {
            RemoveLastRow();
            
            return;
        }

        if (index < 0 || index >= Rows)
            throw new IndexOutOfRangeException();
        
        Rows--;

        for (int i = index; i < Rows; i++) {
            for (int j = 0; j < Columns; j++)
                data[index, j] = data[index + 1, j];
        }

        for (int i = 0; i < Columns; i++)
            data[Rows, i] = default;
    }

    public void RemoveColumn(int index) {
        if (index == Columns - 1) {
            RemoveLastColumn();
            
            return;
        }

        if (index < 0 || index >= Columns)
            throw new IndexOutOfRangeException();

        Columns--;

        for (int i = 0; i < Rows; i++) {
            for (int j = index; j < Columns; j++)
                data[i, index] = data[i, index + 1];
        }

        for (int i = 0; i < Rows; i++)
            data[i, Columns] = default;
    }

    public void SetSize(int rows, int columns) {
        while (Rows < rows)
            AddRow();
        
        while (Rows > rows)
            RemoveLastRow();
        
        while (Columns < columns)
            AddColumn();
        
        while (Columns > columns)
            RemoveLastColumn();
    }

    private void EnsureCapacity(int row, int column) {
        if (rowCapacity >= row && columnCapacity >= column)
            return;

        while (rowCapacity < row)
            rowCapacity *= 2;

        while (columnCapacity < column)
            columnCapacity *= 2;

        var newData = new T[rowCapacity, columnCapacity];

        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Columns; j++)
                newData[i, j] = data[i, j];
        }

        data = newData;
    }
}

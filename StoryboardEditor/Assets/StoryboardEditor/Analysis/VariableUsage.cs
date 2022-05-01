public readonly struct VariableUsage {
    public int Row { get; }
    
    public int Column { get; }
    
    public int TokenIndex { get; }

    public VariableUsage(int row, int column, int tokenIndex) {
        Row = row;
        Column = column;
        TokenIndex = tokenIndex;
    }
}
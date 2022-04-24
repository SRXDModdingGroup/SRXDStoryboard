public class StoryboardDocument {
    public Table<DocumentCellInfo> Content { get; }

    private StoryboardDocument(Table<DocumentCellInfo> content) => Content = content;

    public void SetCellText(int row, int column, string text) {
        Content[row, column] = new DocumentCellInfo(text, text);
    }

    public void InsertRow(int index) => Content.InsertRow(index);

    public static StoryboardDocument CreateNew(int rowCount, int columnCount) => new(new Table<DocumentCellInfo>(rowCount, columnCount));
}

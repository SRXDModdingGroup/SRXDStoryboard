using System;
using System.IO;
using StoryboardSystem;

public class StoryboardDocument {
    public Table<string> Content { get; }

    private StoryboardDocument(Table<string> content) => Content = content;
    
    public void BeginEdit() { }

    public void EndEdit() {
        for (int i = Content.Columns - 1; i >= 0; i--) {
            bool anyInColumn = false;

            for (int j = 0; j < Content.Rows; j++) {
                if (string.IsNullOrWhiteSpace(Content[j, i]))
                    continue;
                
                anyInColumn = true;
                    
                break;
            }

            if (anyInColumn) {
                if (i == Content.Columns - 1)
                    Content.AddColumn();
                
                return;
            }
            
            if (i < Content.Columns - 1)
                Content.RemoveLastColumn();
        }
    }

    public void SetCellText(int row, int column, string text) => Content[row, column] = text;

    public void InsertRow(int index) => Content.InsertRow(index);

    public void SaveToFile(string path) {
        using var writer = new StreamWriter(File.Create(path));

        for (int i = 0; i < Content.Rows; i++) {
            for (int j = 0; j < Content.Columns; j++) {
                string value = Content[i, j];
                
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                value = value.Trim();
                
                if (j > 0)
                    writer.Write(' ');
                
                writer.Write(value);
            }
            
            writer.WriteLine();
        }
    }

    public static StoryboardDocument CreateNew(int rowCount) => new(new Table<string>(rowCount, 1));

    public static bool TryOpenFile(string path, out StoryboardDocument document) {
        if (!File.Exists(path)) {
            document = null;

            return false;
        }
        
        using var reader = new StreamReader(path);
        var content = new Table<string>();
        int i = 0;

        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            int j = 0;
            
            content.AddRow();

            foreach (string s in Parser.Split(line)) {
                if (content.Columns <= j)
                    content.AddColumn();

                content[i, j] = s;
                j++;
            }

            i++;
        }

        document = new StoryboardDocument(content);

        return true;
    }
}

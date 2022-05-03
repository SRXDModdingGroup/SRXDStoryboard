using System.IO;
using StoryboardSystem;

public static class StoryboardDocument {
    public static void Optimize(Table<string> document) {
        for (int i = document.Columns - 1; i >= 0; i--) {
            bool anyInColumn = false;

            for (int j = 0; j < document.Rows; j++) {
                string value = document[i, j];

                if (value == null) {
                    document[i, j] = string.Empty;
                    
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(value))
                    continue;
                
                anyInColumn = true;
                    
                break;
            }

            if (anyInColumn)
                return;

            if (document.Columns > i + 1)
                document.RemoveLastColumn();
        }
    }

    public static void SaveToFile(Table<string> document, string path) {
        using var writer = new StreamWriter(File.Create(path));

        for (int i = 0; i < document.Rows; i++) {
            for (int j = 0; j < document.Columns; j++) {
                string value = document[i, j];
                
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

    public static Table<string> CreateNew(int rowCount) {
        var document = new Table<string>(rowCount, 1);

        for (int i = 0; i < rowCount; i++)
            document[i, 0] = string.Empty;

        return document;
    }

    public static bool TryOpenFile(string path, out Table<string> document) {
        if (!File.Exists(path)) {
            document = null;

            return false;
        }
        
        document = new Table<string>();
        
        using var reader = new StreamReader(path);
        int i = 0;
        
        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            int j = 0;
            
            document.AddRow();

            foreach (string s in Parser.Split(new StringRange(line))) {
                if (document.Columns <= j + 1)
                    document.AddColumn();

                document[i, j] = s;
                j++;
            }

            i++;
        }

        return true;
    }
}

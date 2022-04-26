using System;
using System.Threading;
using System.Threading.Tasks;
using StoryboardSystem;

public class DocumentAnalysis {
    public Table<CellAnalysis> Cells { get; } = new();

    private CancellationTokenSource ctSource;

    public void Analyse(StoryboardDocument document, Action callback) {
        ctSource = new CancellationTokenSource();

        var ct = ctSource.Token;
        
        Task.Run(() => AnalyzeAsync(document, callback, ct), ct);
    }

    public void Cancel() => ctSource?.Cancel();

    private void AnalyzeAsync(StoryboardDocument document, Action callback, CancellationToken ct) {
        var content = document.Content;

        Cells.SetSize(content.Rows, content.Columns);

        for (int i = 0; i < content.Rows; i++) {
            for (int j = 0; j < content.Columns; j++) {
                if (ct.IsCancellationRequested)
                    return;

                string value = content[i, j];

                if (string.IsNullOrWhiteSpace(value)) {
                    Cells[i, j] = new CellAnalysis(string.Empty, null, false);
                        
                    continue;
                }

                value = value.Trim();

                if (value.StartsWith("//")) {
                    Cells[i, j] = new CellAnalysis(value, null, false);
                        
                    continue;
                }
                
                if (Parser.TryParseToken(value, i + 1, out var token))
                    Cells[i, j] = new CellAnalysis(value, token, false);
                else
                    Cells[i, j] = new CellAnalysis(value, null, true);
            }
        }
        
        callback?.Invoke();
    }
}

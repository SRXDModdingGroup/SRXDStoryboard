using System;

public readonly struct DocumentCellInfo {
    public string Text { get; }
    
    public string FormattedText { get; }

    public DocumentCellInfo(string text, string formattedText) {
        Text = text;
        FormattedText = formattedText;
    }
}

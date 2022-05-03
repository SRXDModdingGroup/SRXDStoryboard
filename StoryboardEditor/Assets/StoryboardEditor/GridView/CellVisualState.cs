using UnityEngine;

public class CellVisualState {
    public string Text { get; set; }
    
    public string FormattedText { get; set; }

    public bool IsError { get; set; }
    
    public bool IsProcedureBorder { get; set; }
    
    public Color Color { get; set; }

    public CellVisualState(Color color) : this(string.Empty, color) { }

    public CellVisualState(string text, Color color) {
        Text = text;
        FormattedText = text;
        IsError = false;
        Color = color;
    }
}

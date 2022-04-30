public class CellVisualState {
    public string Text { get; set; }
    
    public string FormattedText { get; set; }

    public bool IsError { get; set; }

    public CellVisualState() : this(string.Empty) { }

    public CellVisualState(string text) {
        Text = text;
        FormattedText = text;
        IsError = false;
    }
}

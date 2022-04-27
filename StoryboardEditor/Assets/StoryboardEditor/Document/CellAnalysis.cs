using StoryboardSystem;

public readonly struct CellAnalysis {
    public string Text { get; }
    
    public string FormattedText { get; }
    
    public Token Token { get; }
    
    public bool IsError { get; }

    public CellAnalysis(string text, string formattedText, Token token, bool isError) {
        Text = text;
        FormattedText = formattedText;
        Token = token;
        IsError = isError;
    }
}
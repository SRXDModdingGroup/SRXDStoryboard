using StoryboardSystem;

public class CellAnalysis {
    public string Text { get; }
    
    public string FormattedText { get; set; }
    
    public Token Token { get; }
    
    public bool IsError { get; set; }

    public CellAnalysis(string text, string formattedText, Token token, bool isError) {
        Text = text;
        FormattedText = formattedText;
        Token = token;
        IsError = isError;
    }
}
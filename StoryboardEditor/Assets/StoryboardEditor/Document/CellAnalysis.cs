using StoryboardSystem;

public readonly struct CellAnalysis {
    public string Text { get; }
    
    public Token Token { get; }
    
    public bool IsError { get; }

    public CellAnalysis(string text, Token token, bool isError) {
        Text = text;
        Token = token;
        IsError = isError;
    }
}
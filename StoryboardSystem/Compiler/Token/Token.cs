namespace StoryboardSystem;

public abstract class Token {
    public abstract TokenType Type { get; }

    public bool Success { get; set; } = true;

    public string FormattedText { get; set; } = string.Empty;

    public StringRange Range { get; set; } = new(string.Empty);
}
namespace StoryboardSystem; 

internal class Name {
    private readonly string name;

    public Name(string name) => this.name = name;

    public override bool Equals(object obj) => obj is Name other && this == other;

    public override int GetHashCode() => name.GetHashCode();

    public override string ToString() => name;

    public static bool operator ==(Name a, Name b) => a?.name == b?.name;
    
    public static bool operator !=(Name a, Name b) => a?.name != b?.name;
}
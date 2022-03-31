namespace SRXDStoryboard.Plugin; 

public readonly struct Name {
    private readonly string name;

    public Name(string name) => this.name = name;

    public override bool Equals(object obj) => obj is Name other && name == other.name;

    public override int GetHashCode() => name.GetHashCode();

    public override string ToString() => name;
}
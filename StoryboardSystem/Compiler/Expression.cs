namespace StoryboardSystem; 

internal class Expression {
    public string Name { get; }
    
    public object[] Arguments { get; }

    public Expression(string name, object[] arguments) {
        Name = name;
        Arguments = arguments;
    }
}
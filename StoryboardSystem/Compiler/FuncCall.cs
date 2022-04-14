namespace StoryboardSystem; 

internal class FuncCall {
    public FuncName Name { get; }
    
    public object[] Arguments { get; }

    public FuncCall(FuncName name, object[] arguments) {
        Name = name;
        Arguments = arguments;
    }
}
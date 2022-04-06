﻿namespace StoryboardSystem; 

internal class Expression {
    public Name Name { get; }
    
    public object[] Arguments { get; }

    public Expression(Name name, object[] arguments) {
        Name = name;
        Arguments = arguments;
    }
}
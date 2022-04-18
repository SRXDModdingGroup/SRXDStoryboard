using System;

namespace StoryboardSystem; 

public abstract class Controller { }

public abstract class Controller<T> : Controller {
    public abstract void Evaluate(float time, Action<T> set);
}
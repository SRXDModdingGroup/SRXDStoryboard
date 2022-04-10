using System;

namespace StoryboardSystem; 

public interface IController { }

public interface IController<T> : IController {
    void Evaluate(float time, Action<T> set);
}
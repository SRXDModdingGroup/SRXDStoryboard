namespace StoryboardSystem; 

internal abstract class Binding {
    public abstract bool IsEvent { get; }
    
    public abstract void Evaluate(float time);
}

internal class Binding<T> : Binding {
    public override bool IsEvent { get; }
    
    private Property<T>[] properties;
    private IController<T> controller;

    public Binding(bool isEvent, Property<T>[] properties, IController<T> controller) {
        IsEvent = isEvent;
        this.properties = properties;
        this.controller = controller;
    }

    public override void Evaluate(float time) {
        foreach (var property in properties)
            controller.Evaluate(time, property.Set);
    }
}
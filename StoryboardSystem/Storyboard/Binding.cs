namespace StoryboardSystem; 

internal abstract class Binding {
    public abstract bool IsEvent { get; }
    
    public abstract void Evaluate(float time);
}

internal class Binding<T> : Binding {
    public override bool IsEvent { get; }
    
    private Property<T>[] properties;
    private Controller<T> controller;

    public Binding(Property<T>[] properties, Controller<T> controller) {
        IsEvent = properties[0].IsEvent;
        this.properties = properties;
        this.controller = controller;
    }

    public override void Evaluate(float time) {
        foreach (var property in properties)
            controller.Evaluate(time, property.Set);
    }
}
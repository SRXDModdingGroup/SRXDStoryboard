namespace StoryboardSystem; 

internal class PostProcessingEnabledProperty : ValueProperty<bool> {
    private PostProcessingInstance instance;

    public PostProcessingEnabledProperty(PostProcessingInstance instance) => this.instance = instance;

    protected internal override void Reset() { }

    protected internal override void Set(bool value) => instance.SetEnabled(value);

    protected internal override bool TryConvert(object value, out bool result) {
        switch (value) {
            case bool boolVal:
                result = boolVal;

                return true;
            case int intVal:
                result = intVal > 0;

                return true;
        }

        result = false;

        return false;
    }

    protected internal override bool Interpolate(bool a, bool b, float t) {
        if (t >= 0.5f)
            return b;

        return a;
    }
}
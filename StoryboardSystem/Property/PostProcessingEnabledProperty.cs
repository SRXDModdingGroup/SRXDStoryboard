namespace StoryboardSystem; 

internal class PostProcessingEnabledProperty : ValueProperty<bool> {
    private LoadedPostProcessingReference reference;

    public PostProcessingEnabledProperty(LoadedPostProcessingReference reference) => this.reference = reference;

    public override void Set(bool value) => reference.SetEnabled(value);

    public override bool TryConvert(object value, out bool result) {
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

    protected override bool Interp(bool a, bool b, float t) {
        if (t >= 0.5f)
            return b;

        return a;
    }
}
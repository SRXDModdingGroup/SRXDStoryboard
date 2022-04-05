namespace StoryboardSystem; 

internal class PostProcessingEnabledProperty : ValueProperty<bool> {
    private LoadedPostProcessingMaterialReference reference;

    public PostProcessingEnabledProperty(LoadedPostProcessingMaterialReference reference) => this.reference = reference;

    public override void Set(bool value) => reference.Enabled = value;

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

    public override bool Interp(bool a, bool b, float t) {
        if (t >= 0.5f)
            return b;

        return a;
    }
}
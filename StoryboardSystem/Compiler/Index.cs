namespace StoryboardSystem; 

internal class Index {
    private object[] array;
    private int index;

    public Index(object[] array, int index) {
        this.array = array;
        this.index = index;
    }

    public bool TrySetArrayValue(object obj) {
        if (array.Length > 0) {
            array[MathUtility.Mod(index, array.Length)] = obj;

            return true;
        }

        StoryboardManager.Instance.Logger.LogWarning($"Array length can not be 0");

        return false;
    }

    public bool TryResolve(out object obj) {
        if (array.Length > 0) {
            obj = array[MathUtility.Mod(index, array.Length)];

            return true;
        }

        StoryboardManager.Instance.Logger.LogWarning($"Array length can not be 0");
        obj = null;

        return false;
    }
}
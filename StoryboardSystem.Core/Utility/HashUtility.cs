namespace StoryboardSystem.Core; 

internal static class HashUtility {
    private const uint HASH_BIAS = 2166136261u;
    private const int HASH_COEFF = 486187739;

    public static int Combine(object a, object b) {
        unchecked {
            int hash = (int) HASH_BIAS * HASH_COEFF ^ a.GetHashCode();

            return hash * HASH_COEFF ^ b.GetHashCode();
        }
    }

    public static int Combine(params object[] objs) {
        unchecked {
            int hash = (int) HASH_BIAS;

            foreach (object o in objs)
                hash = hash * HASH_COEFF ^ o.GetHashCode();

            return hash;
        }
    }
}
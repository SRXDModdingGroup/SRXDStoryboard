namespace StoryboardSystem; 

internal static class MathUtility {
    public static int Mod(int a, int b) => (a % b + b) % b;
}
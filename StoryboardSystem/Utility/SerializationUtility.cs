using System.IO;

namespace StoryboardSystem; 

internal static class SerializationUtility {
    public static bool TrySerialize(object obj, BinaryWriter writer) {
        switch (obj) {
            
            default:
                return false;
        }

        return true;
    }
}
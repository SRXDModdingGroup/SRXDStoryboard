using System;
using System.IO;

namespace StoryboardSystem; 

internal static class SerializationUtility {
    private enum SerializableType {
        Null,
        Bool,
        Int,
        Float,
        String,
        Array
    }
    
    public static bool TrySerialize(object obj, BinaryWriter writer) {
        switch (obj) {
            case bool val:
                writer.Write((byte) SerializableType.Bool);
                writer.Write(val);
                return true;
            case int val:
                writer.Write((byte) SerializableType.Int);
                writer.Write(val);
                return true;
            case float val:
                writer.Write((byte) SerializableType.Float);
                writer.Write(val);
                return true;
            case string val:
                writer.Write((byte) SerializableType.String);
                writer.Write(val);
                return true;
            case object[] arr:
                writer.Write((byte) SerializableType.Array);
                writer.Write(arr.Length);

                foreach (object val in arr) {
                    if (!TrySerialize(val, writer))
                        return false;
                }

                return true;
            default:
                return false;
        }
    }

    public static bool TryDeserialize(BinaryReader reader, out object obj) {
        switch ((SerializableType) reader.ReadByte()) {
            case SerializableType.Null:
                obj = null;
                return true;
            case SerializableType.Bool:
                obj = reader.ReadBoolean();
                return true;
            case SerializableType.Int:
                obj = reader.ReadInt32();
                return true;
            case SerializableType.Float:
                obj = reader.ReadSingle();
                return true;
            case SerializableType.String:
                obj = reader.ReadString();
                return true;
            case SerializableType.Array:
                int length = reader.ReadInt32();
                object[] arr = new object[length];

                for (int i = 0; i < length; i++) {
                    if (TryDeserialize(reader, out arr[i]))
                        continue;
                    
                    obj = null;

                    return false;
                }

                obj = arr;

                return true;
            default:
                obj = null;

                return false;
        }
    }
}
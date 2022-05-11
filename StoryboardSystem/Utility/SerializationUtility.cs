using System.IO;

namespace StoryboardSystem; 

internal static class SerializationUtility {
    private const int VERY_SHORT_ARRAY_LENGTH = 64;
    
    private enum SerializableType {
        Null,
        False,
        True,
        IntAsByte,
        IntAsSByte,
        IntAsUShort,
        IntAsShort,
        Int,
        Float,
        String,
        VeryShortArray,
        ShortArray = VeryShortArray + VERY_SHORT_ARRAY_LENGTH,
        Array
    }

    public static bool TryWrite(this BinaryWriter writer, object obj) {
        switch (obj) {
            case null:
                writer.Write((byte) SerializableType.Null);
                return true;
            case bool val:
                if (val)
                    writer.Write((byte) SerializableType.True);
                else
                    writer.Write((byte) SerializableType.False);
                
                return true;
            case int val:
                switch (val) {
                    case >= byte.MinValue and <= byte.MaxValue:
                        writer.Write((byte) SerializableType.IntAsByte);
                        writer.Write((byte) val);
                        return true;
                    case >= sbyte.MinValue and <= sbyte.MaxValue:
                        writer.Write((byte) SerializableType.IntAsSByte);
                        writer.Write((sbyte) val);
                        return true;
                    case >= ushort.MinValue and <= ushort.MaxValue:
                        writer.Write((byte) SerializableType.IntAsUShort);
                        writer.Write((ushort) val);
                        return true;
                    case >= short.MinValue and <= short.MaxValue:
                        writer.Write((byte) SerializableType.IntAsShort);
                        writer.Write((short) val);
                        return true;
                    default:
                        writer.Write((byte) SerializableType.Int);
                        writer.Write(val);
                        return true;
                }
            case float val:
                writer.Write((byte) SerializableType.Float);
                writer.Write(val);
                return true;
            case string val:
                writer.Write((byte) SerializableType.String);
                writer.Write(val);
                return true;
            case object[] arr:
                int length = arr.Length;
                
                switch (length) {
                    case < VERY_SHORT_ARRAY_LENGTH:
                        writer.Write((byte) (SerializableType.VeryShortArray + length));
                        break;
                    case <= byte.MaxValue:
                        writer.Write((byte) SerializableType.ShortArray);
                        writer.Write((byte) arr.Length);
                        break;
                    case <= ushort.MaxValue:
                        writer.Write((byte) SerializableType.Array);
                        writer.Write((ushort) arr.Length);
                        break;
                    default:
                        StoryboardManager.Instance.Logger.LogWarning($"Max array length is {ushort.MaxValue}");

                        return false;
                }

                foreach (object val in arr) {
                    if (!writer.TryWrite(val))
                        return false;
                }

                return true;
            default:
                StoryboardManager.Instance.Logger.LogWarning($"{obj.GetType()} is not a serializable type");
                
                return false;
        }
    }

    public static bool TryRead(this BinaryReader reader, out object obj) {
        var value = (SerializableType) reader.ReadByte();
        
        switch (value) {
            case SerializableType.Null:
                obj = null;
                return true;
            case SerializableType.False:
                obj = false;
                return true;
            case SerializableType.True:
                obj = true;
                return true;
            case SerializableType.IntAsByte:
                obj = (int) reader.ReadByte();
                return true;
            case SerializableType.IntAsSByte:
                obj = (int) reader.ReadSByte();
                return true;
            case SerializableType.IntAsUShort:
                obj = (int) reader.ReadUInt16();
                return true;
            case SerializableType.IntAsShort:
                obj = (int) reader.ReadInt16();
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
            case <= SerializableType.Array:
                int length = value switch {
                    SerializableType.Array => reader.ReadUInt16(),
                    SerializableType.ShortArray => reader.ReadByte(),
                    _ => value - SerializableType.VeryShortArray
                };

                object[] arr = new object[length];

                for (int i = 0; i < length; i++) {
                    if (reader.TryRead(out arr[i]))
                        continue;
                    
                    obj = null;

                    return false;
                }

                obj = arr;

                return true;
            default:
                obj = null;
                StoryboardManager.Instance.Logger.LogWarning($"{value} is not a valid type tag");

                return false;
        }
    }
}
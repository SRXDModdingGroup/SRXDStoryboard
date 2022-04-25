using System.IO;

namespace StoryboardSystem; 

internal static class SerializationUtility {
    private const int OFFSET_BITS = 8;
    private const int LENGTH_BITS = 7;
    private const int SEARCH_LENGTH = 1 << OFFSET_BITS;
    private const int LOOKAHEAD_LENGTH = (1 << LENGTH_BITS) - 1;
    private const int WINDOW_LENGTH = SEARCH_LENGTH + LOOKAHEAD_LENGTH;
    
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
        Array0,
        Array1,
        Array2,
        Array3,
        Array4,
        Array
    }

    public static void LZSSCompress(Stream fromStream, Stream toStream) {
        fromStream.Position = 0;
        toStream.Position = 0;

        int[] window = new int[WINDOW_LENGTH];
        int windowPosition = 0;

        for (int i = 0; i < SEARCH_LENGTH; i++)
            WindowSet(-1);

        for (int i = 0; i < LOOKAHEAD_LENGTH; i++)
            WindowSet(fromStream.ReadByte());

        while (WindowGet(SEARCH_LENGTH) >= 0) {
            uint offset = 0;
            uint length = 0;

            for (uint i = 0; i < SEARCH_LENGTH; i++) {
                uint newLength = 0;
                
                for (uint k = i, l = SEARCH_LENGTH; l < WINDOW_LENGTH; k++, l++) {
                    if (WindowGet(k) != WindowGet(l))
                        break;

                    newLength++;
                }

                if (newLength < length)
                    continue;
                
                offset = i;
                length = newLength;
                    
                if (length == LOOKAHEAD_LENGTH)
                    break;
            }

            if (length <= 2) {
                toStream.WriteByte(0);
                toStream.WriteByte((byte) WindowGet(SEARCH_LENGTH));
                WindowSet(fromStream.ReadByte());
                
                continue;
            }

            for (int i = 0; i < length; i++)
                WindowSet(fromStream.ReadByte());

            offset |= length << (16 - LENGTH_BITS);
            toStream.WriteByte((byte) (offset >> 8));
            toStream.WriteByte((byte) offset);
        }

        void WindowSet(int value) {
            window[windowPosition % WINDOW_LENGTH] = value;
            windowPosition = (windowPosition + 1) % WINDOW_LENGTH;
        }

        int WindowGet(uint index) => window[(windowPosition + index) % WINDOW_LENGTH];
    }

    public static void LZSSDecompress(Stream fromStream, Stream toStream) {
        fromStream.Position = 0;
        toStream.Position = 0;
        
        int[] buffer = new int[SEARCH_LENGTH];
        int bufferPosition = 0;

        while (fromStream.Position < fromStream.Length) {
            byte byteA = (byte) fromStream.ReadByte();
            byte byteB = (byte) fromStream.ReadByte();
            
            if (byteA == 0) {
                toStream.WriteByte(byteB);
                BufferSet(byteB);
                
                continue;
            }

            uint offset = (uint) (byteA << 8) | byteB;
            uint length = (offset >> (16 - LENGTH_BITS)) & LOOKAHEAD_LENGTH;

            offset &= SEARCH_LENGTH - 1;

            for (int i = 0; i < length; i++) {
                int value = BufferGet(offset);
                
                toStream.WriteByte((byte) value);
                BufferSet(value);
            }
        }
        
        void BufferSet(int value) {
            buffer[bufferPosition % SEARCH_LENGTH] = value;
            bufferPosition = (bufferPosition + 1) % SEARCH_LENGTH;
        }

        int BufferGet(uint index) => buffer[(bufferPosition + index) % SEARCH_LENGTH];
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

                if (length > 4) {
                    writer.Write((byte) SerializableType.Array);
                    writer.Write((ushort) arr.Length);
                }
                else
                    writer.Write((byte) (SerializableType.Array0 + length));

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
            case SerializableType.Array0:
            case SerializableType.Array1:
            case SerializableType.Array2:
            case SerializableType.Array3:
            case SerializableType.Array4:
            case SerializableType.Array:
                int length;

                if (value == SerializableType.Array)
                    length = reader.ReadUInt16();
                else
                    length = value - SerializableType.Array0;
                
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
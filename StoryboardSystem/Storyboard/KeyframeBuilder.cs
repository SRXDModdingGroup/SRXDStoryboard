using System.IO;

namespace StoryboardSystem; 

internal readonly struct KeyframeBuilder {
    private readonly Timestamp time;
    private readonly object value;
    private readonly InterpType interpType;
    private readonly int order;

    public KeyframeBuilder(Timestamp time, object value, InterpType interpType, int order) {
        this.time = time;
        this.value = value;
        this.interpType = interpType;
        this.order = order;
    }

    public bool TryCreateKeyframe<T>(Property<T> property, IStoryboardParams sParams, out Keyframe<T> result) {
        if (!property.TryConvert(value, out var converted)) {
            result = default;

            return false;
        }
        
        result = new Keyframe<T>(sParams.Convert(time.Measures, time.Beats, time.Ticks, time.Seconds), converted, interpType, order);

        return true;
    }

    public bool TrySerialize(BinaryWriter writer) {
        time.Serialize(writer);

        if (!SerializationUtility.TrySerialize(value, writer))
            return false;
        
        writer.Write((byte) interpType);
        writer.Write(order);

        return true;
    }

    public static bool TryDeserialize(BinaryReader reader, out KeyframeBuilder keyframeBuilder) {
        var time = Timestamp.Deserialize(reader);

        if (!SerializationUtility.TryDeserialize(reader, out object value)) {
            keyframeBuilder = default;
            
            return false;
        }

        var interpType = (InterpType) reader.ReadByte();
        int order = reader.ReadInt32();

        keyframeBuilder = new KeyframeBuilder(time, value, interpType, order);

        return true;
    }
}
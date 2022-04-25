using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryboardSystem; 

internal class TimelineBuilder {
    private static readonly float NEAR_KEY_THRESHOLD = 0.001f;
    
    public string Name { get; }
    
    private List<KeyframeBuilder> keyframeBuilders;
    
    public TimelineBuilder(string name) {
        Name = name;
        keyframeBuilders = new List<KeyframeBuilder>();
    }

    private TimelineBuilder(string name, List<KeyframeBuilder> keyframeBuilders) {
        Name = name;
        this.keyframeBuilders = keyframeBuilders;
    }

    public void AddKey(Timestamp time, object value, InterpType interpType)
        => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType));

    public bool TryCreateController(Property property, IStoryboardParams sParams, out Controller controller)
        => property.TryCreateTimeline(this, sParams, out controller);

    public bool TryCreateController<T>(Property<T> property, IStoryboardParams sParams, out Controller controller) {
        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframeBuilders.Count; i++) {
            var builder = keyframeBuilders[i];
            
            if (!property.TryConvert(builder.Value, out var converted)) {
                controller = null;

                return false;
            }
            
            keyframes[i] = new Keyframe<T>(sParams.Convert((float) builder.Time.Measures, (float) builder.Time.Beats, (float) builder.Time.Ticks, (float) builder.Time.Seconds), converted, builder.InterpType, i);
        }

        if (keyframes.Length > 1) {
            Array.Sort(keyframes);

            int runStartIndex = 0;
            var sortingTimes = new List<float>();

            for (int i = 0; i < keyframes.Length; i++) {
                float time = keyframes[i].Time;
            
                if (i < keyframes.Length - 1 && keyframes[i + 1].Time - time < NEAR_KEY_THRESHOLD)
                    continue;
                
                if (i > runStartIndex) {
                    sortingTimes.Clear();
                    
                    for (int j = 0, k = runStartIndex; j <= i - runStartIndex; j++, k++)
                        sortingTimes.Add(keyframes[k].Time);
            
                    bool sorted;
            
                    do {
                        sorted = true;
            
                        for (int j = 0, k = runStartIndex; j < sortingTimes.Count - 1; j++, k++) {
                            var first = keyframes[k];
                            var second = keyframes[k + 1];
            
                            if (first.Order <= second.Order
                                || Mathf.Abs(first.Time - sortingTimes[k + 1]) >= NEAR_KEY_THRESHOLD
                                || Mathf.Abs(second.Time - sortingTimes[k]) >= NEAR_KEY_THRESHOLD)
                                continue;
                            
                            keyframes[k] = second;
                            keyframes[k + 1] = first;
                            sorted = false;
                        }
                    } while (!sorted);
            
                    for (int j = 0, k = runStartIndex; j < sortingTimes.Count; j++, k++) {
                        var keyframe = keyframes[k];
            
                        keyframes[k] = new Keyframe<T>(sortingTimes[j], keyframe.Value, keyframe.InterpType, 0);
                    }
                }
            
                runStartIndex = i + 1;
            }
        }

        if (property.IsEvent)
            controller = new EventController<T>(keyframes);
        else
            controller = new CurveController<T>(keyframes, property.Interpolate);

        return true;
    }
    
    public bool TrySerialize(BinaryWriter writer) {
        writer.Write(Name);
        writer.Write(keyframeBuilders.Count);
        
        var previous = Timestamp.Zero;
        using var buffer = new BinaryWriter(new MemoryStream(16));

        foreach (var keyframeBuilder in keyframeBuilders) {
            (keyframeBuilder.Time - previous).Serialize(writer, buffer);
            previous = keyframeBuilder.Time;
        }

        foreach (var keyframeBuilder in keyframeBuilders) {
            if (!writer.TryWrite(keyframeBuilder.Value))
                return false;
        }

        foreach (var keyframeBuilder in keyframeBuilders)
            writer.Write((byte) keyframeBuilder.InterpType);

        return true;
    }

    public static bool TryDeserialize(BinaryReader reader, out TimelineBuilder builder) {
        string name = reader.ReadString();
        int keyframeBuildersCount = reader.ReadInt32();
        var keyframeBuilders = new List<KeyframeBuilder>(keyframeBuildersCount);
        var times = new Timestamp[keyframeBuildersCount];
        var previous = Timestamp.Zero;

        for (int i = 0; i < keyframeBuildersCount; i++) {
            times[i] = Timestamp.Deserialize(reader) + previous;
            previous = times[i];
        }

        object[] values = new object[keyframeBuildersCount];
        
        for (int i = 0; i < keyframeBuildersCount; i++) {
            if (reader.TryRead(out values[i]))
                continue;
            
            builder = null;
                
            return false;
        }

        for (int i = 0; i < keyframeBuildersCount; i++) {
            var interpType = (InterpType) reader.ReadByte();
            var keyframeBuilder = new KeyframeBuilder(times[i], values[i], interpType);

            keyframeBuilders.Add(keyframeBuilder);
        }

        builder = new TimelineBuilder(name, keyframeBuilders);

        return true;
    }
}
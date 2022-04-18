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

    public void AddKey(Timestamp time, object value, InterpType interpType, int order) => keyframeBuilders.Add(new KeyframeBuilder(time, value, interpType, order));

    public bool TryCreateController(Property property, IStoryboardParams sParams, out Controller controller)
        => property.TryCreateTimeline(this, sParams, out controller);

    public bool TryCreateController<T>(Property<T> property, IStoryboardParams sParams, out Controller controller) {
        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframeBuilders.Count; i++) {
            if (keyframeBuilders[i].TryCreateKeyframe(property, sParams, out keyframes[i]))
                continue;
            
            controller = null;

            return false;
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

        foreach (var keyframeBuilder in keyframeBuilders) {
            if (!keyframeBuilder.TrySerialize(writer))
                return false;
        }

        return true;
    }

    public static bool TryDeserialize(BinaryReader reader, out TimelineBuilder timelineBuilder) {
        string name = reader.ReadString();
        int keyframeBuildersCount = reader.ReadInt32();
        var keyframeBuilders = new List<KeyframeBuilder>(keyframeBuildersCount);

        for (int i = 0; i < keyframeBuildersCount; i++) {
            if (!KeyframeBuilder.TryDeserialize(reader, out var keyframeBuilder)) {
                timelineBuilder = null;

                return false;
            }
            
            keyframeBuilders.Add(keyframeBuilder);
        }

        timelineBuilder = new TimelineBuilder(name, keyframeBuilders);

        return true;
    }
}
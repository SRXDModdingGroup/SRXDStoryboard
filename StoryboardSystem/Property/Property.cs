using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

internal abstract class Property {
    public abstract bool TryCreateTimeline(Property[] properties, List<KeyframeBuilder> keyframeBuilders, ITimeConversion conversion, out Timeline timeline);
}

internal abstract class Property<T> : Property {
    public abstract void Set(T value);

    public abstract bool TryConvert(object value, out T result);

    public override bool TryCreateTimeline(Property[] properties, List<KeyframeBuilder> keyframeBuilders, ITimeConversion conversion, out Timeline timeline) {
        var propertiesT = new Property<T>[properties.Length];

        for (int i = 0; i < properties.Length; i++) {
            if (properties[i] is not Property<T> property) {
                timeline = null;

                return false;
            }

            propertiesT[i] = property;
        }

        var keyframes = new Keyframe<T>[keyframeBuilders.Count];

        for (int i = 0; i < keyframeBuilders.Count; i++) {
            if (keyframeBuilders[i].TryCreateKeyframe(this, conversion, out keyframes[i]))
                continue;
            
            timeline = null;

            return false;
        }

        Array.Sort(keyframes);
        timeline = new Curve<T>(propertiesT, keyframes);

        return true;
    }
}
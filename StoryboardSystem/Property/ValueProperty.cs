using System;
using System.Collections.Generic;

namespace StoryboardSystem;

internal abstract class ValueProperty<T> : Property<T> {
    public abstract T Interp(T a, T b, float t);
}
using UnityEngine;

namespace StoryboardSystem.Core; 

internal interface IVectorConversion<out T> {
    T Convert(VectorN value);
}
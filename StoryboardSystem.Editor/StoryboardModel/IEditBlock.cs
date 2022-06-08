using System;
using StoryboardSystem.Core;

namespace StoryboardSystem.Editor;

public interface IEditBlock : IDisposable {
    public void AddPattern(int index, Pattern pattern);

    public void DeletePattern(int index);

    public void MovePattern(int fromIndex, int toIndex);

    public void RenamePattern(int index, string newName);

    public void AddPatternInstance(PatternInstance instance);

    public void RemovePatternInstance(int index);

    public void MovePatternInstance(int index, double time, int lane);

    public void CropPatternInstance(PatternInstance instance, double cropStart, double cropEnd);

    public void AddLane(Pattern pattern, int index, Lane lane);

    public void RemoveLane(Pattern pattern, int index);

    public void AddFrame(Lane lane, Frame frame);

    public void RemoveFrame(Lane lane, int index);

    public void MoveFrame(Lane lane, Frame frame, double time);

    public void ChangeFrameData(Frame frame, FrameData data);

    public void ChangeFrameInterpType(Frame frame, InterpType interpType);

    public void ChangeFrameValue(Frame frame, int valueIndex, ValueData value);
}
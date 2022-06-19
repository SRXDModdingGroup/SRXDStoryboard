using System;

namespace VisualizerSystem.Editor {
    public interface IEditBlock : IDisposable {
        public void AddFrame(Lane lane, Frame frame);

        public void RemoveFrame(Lane lane, int index);

        public void MoveFrame(Lane lane, Frame frame, double time);

        public void ChangeFrameData(Frame frame, FrameData data);

        public void ChangeFrameValue(Frame frame, int valueIndex, ValueData value);
    }
}
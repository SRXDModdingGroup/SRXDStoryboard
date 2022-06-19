using System;
using System.Collections.Generic;

namespace VisualizerSystem.Editor {
    public partial class VisualizerModel {
        private class EditBlock : IEditBlock {
            private VisualizerProject Project;
            private IUndoRedoAction action;
            private Action completed;
            
            private bool disposed;

            public EditBlock(VisualizerProject project, IUndoRedoAction action, Action completed) {
                Project = project;
                this.action = action;
                this.completed = completed;
            }

            public void AddFrame(Lane lane, Frame frame) => AddTimedElement(lane.Frames, frame);

            public void RemoveFrame(Lane lane, int index) => RemoveElement(lane.Frames, index);

            public void MoveFrame(Lane lane, Frame frame, double time) {
                double oldTime = frame.Time;
                var frames = lane.Frames;
                int fromIndex = frames.IndexOf(frame);
                int toIndex = frames.BinarySearch(time);

                if (toIndex < 0)
                    toIndex = ~toIndex;
                
                Do(frame, time);
                action.AddSubAction(() => Do(frame, oldTime), () => Do(frame, time));
                MoveElement(frames, fromIndex, toIndex);

                void Do(Frame frame, double time) => frame.Time = time;
            }

            public void ChangeFrameData(Frame frame, FrameData data) {
                var oldData = frame.Data;
                
                Do(frame, data);
                action.AddSubAction(() => Do(frame, oldData), () => Do(frame, data));

                void Do(Frame frame, FrameData data) => frame.Data = data;
            }

            public void ChangeFrameValue(Frame frame, int valueIndex, ValueData value) {
                var oldValue = frame.Values[valueIndex];
                
                Do(frame, valueIndex, value);
                action.AddSubAction(() => Do(frame, valueIndex, oldValue), () => Do(frame, valueIndex, value));

                void Do(Frame frame, int valueIndex, ValueData value) => frame.Values[valueIndex] = value;
            }

            public void Dispose() {
                if (disposed)
                    return;
                
                action.Dispose();
                completed?.Invoke();
                disposed = true;
            }
            
            private void AddElement<T>(List<T> list, int index, T element) {
                Do(list, index, element);
                action.AddSubAction(() => Undo(list, index), () => Do(list, index, element));

                void Do(List<T> list, int index, T element) => list.Insert(index, element);

                void Undo(List<T> list, int index) => list.RemoveAt(index);
            }

            private void AddTimedElement<T>(List<T> list, T element) where T : ITimedElement => AddElement(list, list.BinarySearch(element.Time), element);

            private void RemoveElement<T>(List<T> list, int index) {
                var element = list[index];
                
                Do(list, index);
                action.AddSubAction(() => Undo(list, index, element), () => Do(list, index));

                void Do(List<T> list, int index) => list.RemoveAt(index);

                void Undo(List<T> list, int index, T element) => list.Insert(index, element);
            }

            private void MoveElement<T>(List<T> list, int fromIndex, int toIndex) {
                Do(list, fromIndex, toIndex);
                action.AddSubAction(() => Do(list, toIndex, fromIndex), () => Do(list, fromIndex, toIndex));
                
                void Do(List<T> list, int fromIndex, int toIndex) {
                    if (toIndex > fromIndex) {
                        list.Insert(toIndex, list[fromIndex]);
                        list.RemoveAt(fromIndex);
                    }
                    else {
                        list.RemoveAt(fromIndex);
                        list.Insert(toIndex, list[fromIndex]);
                    }
                }
            }
        }
    }
}
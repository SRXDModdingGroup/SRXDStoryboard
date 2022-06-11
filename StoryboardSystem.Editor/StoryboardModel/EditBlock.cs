using System;
using System.Collections.Generic;
using StoryboardSystem.Core;

namespace StoryboardSystem.Editor;

public partial class StoryboardModel {
    private class EditBlock : IEditBlock {
        private StoryboardProject Project;
        private IUndoRedoAction action;
        private Action completed;
        
        private bool disposed;

        public EditBlock(StoryboardProject project, IUndoRedoAction action, Action completed) {
            Project = project;
            this.action = action;
            this.completed = completed;
        }
        
        public void AddPattern(int index, Pattern pattern) {
            Do(index, pattern);
            action.AddSubAction(() => Undo(index), () => Do(index, pattern));
            
            void Do(int index, Pattern pattern) => Project.Patterns.Insert(index, pattern);

            void Undo(int index) => Project.Patterns.RemoveAt(index);
        }

        public void DeletePattern(int index) {
            var pattern = Project.Patterns[index];
            var patternInstances = Project.PatternInstances;

            for (int i = patternInstances.Count - 1; i >= 0; i--) {
                if (patternInstances[i].Pattern == pattern)
                    RemovePatternInstance(i);
            }

            Do(index);
            action.AddSubAction(() => Undo(index, pattern), () => Do(index));

            void Do(int patternIndex) => Project.Patterns.RemoveAt(patternIndex);

            void Undo(int patternIndex, Pattern pattern) => Project.Patterns.Insert(patternIndex, pattern);
        }

        public void MovePattern(int fromIndex, int toIndex) => MoveElement(Project.Patterns, fromIndex, toIndex);

        public void RenamePattern(int index, string newName) {
            var pattern = Project.Patterns[index];
            string oldName = pattern.Name;
            
            Do(pattern, newName);
            action.AddSubAction(() => Do(pattern, oldName), () => Do(pattern, newName));

            void Do(Pattern pattern, string newName) => pattern.Name = newName;
        }

        public void AddPatternInstance(PatternInstance instance) => AddSortedElement(Project.PatternInstances, instance);

        public void RemovePatternInstance(int index) {
            var instance = Project.PatternInstances[index];
            
            Do(index);
            action.AddSubAction(() => Undo(index, instance), () => Do(index));

            void Do(int instanceIndex) => Project.PatternInstances.RemoveAt(instanceIndex);

            void Undo(int instanceIndex, PatternInstance instance) => Project.PatternInstances.Insert(instanceIndex, instance);
        }

        public void MovePatternInstance(int index, double time, int lane) {
            var instance = Project.PatternInstances[index];
            double oldTime = instance.Time;
            int oldLane = instance.Lane;
            var patternInstances = Project.PatternInstances;
            int toIndex = patternInstances.BinarySearch(new PatternInstance(null, time, 0d, 0d, lane));

            if (toIndex < 0)
                toIndex = ~toIndex;
            
            Do(instance, time, lane);
            action.AddSubAction(() => Do(instance, oldTime, oldLane), () => Do(instance, time, lane));
            MoveElement(patternInstances, index, toIndex);

            void Do(PatternInstance instance, double time, int lane) {
                instance.Time = time;
                instance.Lane = lane;
            }
        }
        
        public void CropPatternInstance(PatternInstance instance, double cropStart, double cropEnd) {
            double oldCropStart = instance.CropStart;
            double oldCropEnd = instance.CropEnd;
            
            Do(instance, cropStart, cropEnd);
            action.AddSubAction(() => Do(instance, oldCropStart, oldCropEnd), () => Do(instance, cropStart, cropEnd));

            void Do(PatternInstance instance, double cropStart, double cropEnd) {
                instance.CropStart = cropStart;
                instance.CropEnd = cropEnd;
            }
        }

        public void AddLane(Pattern pattern, int index, Lane lane) => AddElement(pattern.Lanes, index, lane);

        public void RemoveLane(Pattern pattern, int index) => RemoveElement(pattern.Lanes, index);

        public void AddFrame(Lane lane, Frame frame) => AddSortedElement(lane.Frames, frame);

        public void RemoveFrame(Lane lane, int index) => RemoveElement(lane.Frames, index);

        public void MoveFrame(Lane lane, Frame frame, double time) {
            double oldTime = frame.Time;
            var frames = lane.Frames;
            int fromIndex = frames.IndexOf(frame);
            int toIndex = frames.BinarySearch(new Frame(time, new FrameData(), null));

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

        private void AddSortedElement<T>(List<T> list, T element) where T : IComparable<T> {
            int index = list.BinarySearch(element);

            if (index < 0)
                index = ~index;
            
            AddElement(list, index, element);
        }

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
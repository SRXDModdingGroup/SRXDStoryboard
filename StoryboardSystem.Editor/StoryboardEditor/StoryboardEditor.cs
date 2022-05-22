using System;
using System.Collections.Generic;
using StoryboardSystem.Core;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardEditor : MonoBehaviour {
    [SerializeField] private StoryboardView view;
    
    private StoryboardProject project;
    private UndoRedo undoRedo;
    
    private void Awake() {
        undoRedo = new UndoRedo();
    }

    private void BeginEdit() {
        undoRedo.BeginNewAction();
    }

    private void EndEdit() {
        undoRedo.CompleteAction();
        UpdateView();
    }

    private void UpdateView() {
        view.UpdateView(new ViewInfo(project, undoRedo.CanRedo(), undoRedo.CanRedo()));
    }

    #region InternalActions

    private void CreateNewProject(ProjectSetup setup) {
        undoRedo.Clear();
        project = new StoryboardProject(setup);
        UpdateView();
    }

    private void AddPattern(int index, Pattern pattern) {
        Do(index, pattern);
        undoRedo.AddSubAction(() => Undo(index), () => Do(index, pattern));
        
        void Do(int index, Pattern pattern) => InsertPattern(index, pattern);

        void Undo(int index) => RemovePattern(index);
    }

    private void DeletePattern(Pattern pattern) {
        int patternIndex = project.Patterns.IndexOf(pattern);
        var patternInstances = project.PatternInstances;

        for (int i = patternInstances.Count - 1; i >= 0; i--) {
            var patternInstance = patternInstances[i];

            if (patternInstance.PatternIndex == patternIndex)
                RemovePatternInstance(patternInstance);
        }

        Do(patternIndex);
        undoRedo.AddSubAction(() => Undo(patternIndex, pattern), () => Do(patternIndex));

        void Do(int patternIndex) => RemovePattern(patternIndex);

        void Undo(int patternIndex, Pattern pattern) => InsertPattern(patternIndex, pattern);
    }

    private void MovePattern(Pattern pattern, int index) => MoveElement(project.Patterns, project.Patterns.IndexOf(pattern), index);

    private void RenamePattern(Pattern pattern, string newName) {
        string oldName = pattern.Name;
        
        Do(pattern, newName);
        undoRedo.AddSubAction(() => Do(pattern, oldName), () => Do(pattern, newName));

        void Do(Pattern pattern, string newName) => pattern.Name = newName;
    }

    private void AddPatternInstance(PatternInstance instance) => AddSortedElement(project.PatternInstances, instance);

    private void RemovePatternInstance(PatternInstance instance) {
        int instanceIndex = project.PatternInstances.IndexOf(instance);
        
        Do(instanceIndex);
        undoRedo.AddSubAction(() => Undo(instanceIndex, instance), () => Do(instanceIndex));

        void Do(int instanceIndex) => project.PatternInstances.RemoveAt(instanceIndex);

        void Undo(int instanceIndex, PatternInstance instance) => project.PatternInstances.Insert(instanceIndex, instance);
    }

    private void MovePatternInstance(PatternInstance instance, double time, int lane) {
        double oldTime = instance.Time;
        int oldLane = instance.Lane;
        var patternInstances = project.PatternInstances;
        int fromIndex = patternInstances.IndexOf(instance);
        int toIndex = patternInstances.BinarySearch(new PatternInstance(0, time, 0d, 0d, lane));

        if (toIndex < 0)
            toIndex = ~toIndex;
        
        Do(instance, time, lane);
        undoRedo.AddSubAction(() => Do(instance, oldTime, oldLane), () => Do(instance, time, lane));
        MoveElement(patternInstances, fromIndex, toIndex);

        void Do(PatternInstance instance, double time, int lane) {
            instance.Time = time;
            instance.Lane = lane;
        }
    }
    
    private void CropPatternInstance(PatternInstance instance, double cropStart, double cropEnd) {
        double oldCropStart = instance.CropStart;
        double oldCropEnd = instance.CropEnd;
        
        Do(instance, cropStart, cropEnd);
        undoRedo.AddSubAction(() => Do(instance, oldCropStart, oldCropEnd), () => Do(instance, cropStart, cropEnd));

        void Do(PatternInstance instance, double cropStart, double cropEnd) {
            instance.CropStart = cropStart;
            instance.CropEnd = cropEnd;
        }
    }

    private void AddLane(Pattern pattern, int index, Lane lane) => AddElement(pattern.Lanes, index, lane);

    private void RemoveLane(Pattern pattern, Lane lane) => RemoveElement(pattern.Lanes, lane);

    private void AddFrame(Lane lane, Frame frame) => AddSortedElement(lane.Frames, frame);

    private void RemoveFrame(Lane lane, Frame frame) => RemoveElement(lane.Frames, frame);

    private void MoveFrame(Lane lane, Frame frame, double time) {
        double oldTime = frame.Time;
        var frames = lane.Frames;
        int fromIndex = frames.IndexOf(frame);
        int toIndex = frames.BinarySearch(new Frame(time, 0, new FrameData(), InterpType.Fixed, null));

        if (toIndex < 0)
            toIndex = ~toIndex;
        
        Do(frame, time);
        undoRedo.AddSubAction(() => Do(frame, oldTime), () => Do(frame, time));
        MoveElement(frames, fromIndex, toIndex);

        void Do(Frame frame, double time) => frame.Time = time;
    }

    private void ChangeFrameData(Frame frame, FrameData data) {
        var oldData = frame.Data;
        
        Do(frame, data);
        undoRedo.AddSubAction(() => Do(frame, oldData), () => Do(frame, data));

        void Do(Frame frame, FrameData data) => frame.Data = data;
    }
    
    private void ChangeFrameInterpType(Frame frame, InterpType interpType) {
        var oldInterpType = frame.InterpType;
        
        Do(frame, interpType);
        undoRedo.AddSubAction(() => Do(frame, oldInterpType), () => Do(frame, interpType));

        void Do(Frame frame, InterpType interpType) => frame.InterpType = interpType;
    }

    private void ChangeFrameValue(Frame frame, int valueIndex, ValueData value) {
        var oldValue = frame.Values[valueIndex];
        
        Do(frame, valueIndex, value);
        undoRedo.AddSubAction(() => Do(frame, valueIndex, oldValue), () => Do(frame, valueIndex, value));

        void Do(Frame frame, int valueIndex, ValueData value) => frame.Values[valueIndex] = value;
    }

    private void AddElement<T>(List<T> list, int index, T element) {
        Do(list, index, element);
        undoRedo.AddSubAction(() => Undo(list, index), () => Do(list, index, element));

        void Do(List<T> list, int index, T element) => list.Insert(index, element);

        void Undo(List<T> list, int index) => list.RemoveAt(index);
    }

    private void AddSortedElement<T>(List<T> list, T element) where T : IComparable<T> {
        int index = list.BinarySearch(element);

        if (index < 0)
            index = ~index;
        
        AddElement(list, index, element);
    }

    private void RemoveElement<T>(List<T> list, T element) {
        int index = list.IndexOf(element);
        
        Do(list, index);
        undoRedo.AddSubAction(() => Undo(list, index, element), () => Do(list, index));

        void Do(List<T> list, int index) => list.RemoveAt(index);

        void Undo(List<T> list, int index, T element) => list.Insert(index, element);
    }

    private void MoveElement<T>(List<T> list, int fromIndex, int toIndex) {
        Do(list, fromIndex, toIndex);
        undoRedo.AddSubAction(() => Do(list, toIndex, fromIndex), () => Do(list, fromIndex, toIndex));
        
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

    #endregion
    
    private void InsertPattern(int patternIndex, Pattern pattern) {
        project.Patterns.Insert(patternIndex, pattern);

        foreach (var patternInstance in project.PatternInstances) {
            if (patternInstance.PatternIndex >= patternIndex)
                patternInstance.PatternIndex++;
        }
    }

    private void RemovePattern(int patternIndex) {
        project.Patterns.RemoveAt(patternIndex);
        
        foreach (var patternInstance in project.PatternInstances) {
            if (patternInstance.PatternIndex > patternIndex)
                patternInstance.PatternIndex--;
        }
    }

    private string GetUniquePatternName() {
        string patternName;
        int index = 0;

        while (true) {
            patternName = $"NewPattern_{index}";

            bool exists = false;

            foreach (var pattern in project.Patterns) {
                if (patternName != pattern.Name)
                    continue;

                exists = true;

                break;
            }
            
            if (!exists)
                break;

            index++;
        }

        return patternName;
    }
}
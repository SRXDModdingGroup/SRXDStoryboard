using System;
using System.Collections.Generic;
using StoryboardSystem.Core;
using StoryboardSystem.Rigging;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class StoryboardModel : MonoBehaviour {
    public StoryboardProject Project { get; private set; }

    public event Action Changed;
    
    private UndoRedo undoRedo;

    public void CreateNewProject(ProjectSetup setup) {
        undoRedo.Clear();
        Project = new StoryboardProject(setup);
    }

    public void BeginEdit() => undoRedo.BeginNewAction();

    public void EndEdit() {
        undoRedo.CompleteAction();
        Changed?.Invoke();
    }

    public void Undo() {
        if (!undoRedo.CanUndo)
            return;
        
        undoRedo.Undo();
        Changed?.Invoke();
    }

    public void Redo() {
        if (!undoRedo.CanRedo)
            return;
        
        undoRedo.Redo();
        Changed?.Invoke();
    }

    public void AddPattern(int index, Pattern pattern) {
        Do(index, pattern);
        undoRedo.AddSubAction(() => Undo(index), () => Do(index, pattern));
        
        void Do(int index, Pattern pattern) => InsertPattern(index, pattern);

        void Undo(int index) => RemovePattern(index);
    }

    public void DeletePattern(int index) {
        var pattern = Project.Patterns[index];
        var patternInstances = Project.PatternInstances;

        for (int i = patternInstances.Count - 1; i >= 0; i--) {
            if (patternInstances[i].PatternIndex == index)
                RemovePatternInstance(i);
        }

        Do(index);
        undoRedo.AddSubAction(() => Undo(index, pattern), () => Do(index));

        void Do(int patternIndex) => RemovePattern(patternIndex);

        void Undo(int patternIndex, Pattern pattern) => InsertPattern(patternIndex, pattern);
    }

    public void MovePattern(int fromIndex, int toIndex) => MoveElement(Project.Patterns, fromIndex, toIndex);

    public void RenamePattern(int index, string newName) {
        var pattern = Project.Patterns[index];
        string oldName = pattern.Name;
        
        Do(pattern, newName);
        undoRedo.AddSubAction(() => Do(pattern, oldName), () => Do(pattern, newName));

        void Do(Pattern pattern, string newName) => pattern.Name = newName;
    }

    public void AddPatternInstance(PatternInstance instance) => AddSortedElement(Project.PatternInstances, instance);

    public void RemovePatternInstance(int index) {
        var instance = Project.PatternInstances[index];
        
        Do(index);
        undoRedo.AddSubAction(() => Undo(index, instance), () => Do(index));

        void Do(int instanceIndex) => Project.PatternInstances.RemoveAt(instanceIndex);

        void Undo(int instanceIndex, PatternInstance instance) => Project.PatternInstances.Insert(instanceIndex, instance);
    }

    public void MovePatternInstance(int index, double time, int lane) {
        var instance = Project.PatternInstances[index];
        double oldTime = instance.Time;
        int oldLane = instance.Lane;
        var patternInstances = Project.PatternInstances;
        int toIndex = patternInstances.BinarySearch(new PatternInstance(0, time, 0d, 0d, lane));

        if (toIndex < 0)
            toIndex = ~toIndex;
        
        Do(instance, time, lane);
        undoRedo.AddSubAction(() => Do(instance, oldTime, oldLane), () => Do(instance, time, lane));
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
        undoRedo.AddSubAction(() => Do(instance, oldCropStart, oldCropEnd), () => Do(instance, cropStart, cropEnd));

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
        int toIndex = frames.BinarySearch(new Frame(time, 0, new FrameData(), InterpType.Fixed, null));

        if (toIndex < 0)
            toIndex = ~toIndex;
        
        Do(frame, time);
        undoRedo.AddSubAction(() => Do(frame, oldTime), () => Do(frame, time));
        MoveElement(frames, fromIndex, toIndex);

        void Do(Frame frame, double time) => frame.Time = time;
    }

    public void ChangeFrameData(Frame frame, FrameData data) {
        var oldData = frame.Data;
        
        Do(frame, data);
        undoRedo.AddSubAction(() => Do(frame, oldData), () => Do(frame, data));

        void Do(Frame frame, FrameData data) => frame.Data = data;
    }
    
    public void ChangeFrameInterpType(Frame frame, InterpType interpType) {
        var oldInterpType = frame.InterpType;
        
        Do(frame, interpType);
        undoRedo.AddSubAction(() => Do(frame, oldInterpType), () => Do(frame, interpType));

        void Do(Frame frame, InterpType interpType) => frame.InterpType = interpType;
    }

    public void ChangeFrameValue(Frame frame, int valueIndex, ValueData value) {
        var oldValue = frame.Values[valueIndex];
        
        Do(frame, valueIndex, value);
        undoRedo.AddSubAction(() => Do(frame, valueIndex, oldValue), () => Do(frame, valueIndex, value));

        void Do(Frame frame, int valueIndex, ValueData value) => frame.Values[valueIndex] = value;
    }

    private void Awake() => undoRedo = new UndoRedo();

    private void Start() {
        CreateNewProject(new ProjectSetup(new RigSetup[] {
            new("test", "Test", 1, RigType.Event, new RigParameterSetup[] {
                new("param", "Param", RigValueType.Int, Vector3.zero, Vector3.zero, Vector3.one, true, true)
            })
        }, new [] { 0d, 1d, 2d, 3d }));
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

    private void RemoveElement<T>(List<T> list, int index) {
        var element = list[index];
        
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
    
    private void InsertPattern(int patternIndex, Pattern pattern) {
        Project.Patterns.Insert(patternIndex, pattern);

        foreach (var patternInstance in Project.PatternInstances) {
            if (patternInstance.PatternIndex >= patternIndex)
                patternInstance.PatternIndex++;
        }
    }

    private void RemovePattern(int patternIndex) {
        Project.Patterns.RemoveAt(patternIndex);
        
        foreach (var patternInstance in Project.PatternInstances) {
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

            foreach (var pattern in Project.Patterns) {
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
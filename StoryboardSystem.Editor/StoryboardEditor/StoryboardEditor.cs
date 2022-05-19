using System;
using System.Collections.Generic;
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

    private void CreateNewProject(ProjectSetup setup) {
        undoRedo.Clear();
        project = new StoryboardProject(setup);
        UpdateView();
    }

    private void CreateNewPattern(int patternIndex, string patternName) {
        var newPattern = new Pattern(patternName, project.Setup);
        
        Do(patternIndex, newPattern);
        undoRedo.AddSubAction(() => Undo(patternIndex), () => Do(patternIndex, newPattern));
        
        void Do(int patternIndex, Pattern pattern) => InsertPattern(patternIndex, pattern);

        void Undo(int patternIndex) => RemovePattern(patternIndex);
    }

    private void RenamePattern(Pattern pattern, string newName) {
        string oldName = pattern.Name;
        
        Do(pattern, newName);
        undoRedo.AddSubAction(() => Do(pattern, oldName), () => Do(pattern, newName));

        void Do(Pattern pattern, string newName) => pattern.Name = newName;
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

    private void AddPatternInstance(PatternInstance instance) => AddSortedElement(project.PatternInstances, instance);

    private void RemovePatternInstance(PatternInstance instance) {
        int instanceIndex = project.PatternInstances.IndexOf(instance);
        
        Do(instanceIndex);
        undoRedo.AddSubAction(() => Undo(instanceIndex, instance), () => Do(instanceIndex));

        void Do(int instanceIndex) => project.PatternInstances.RemoveAt(instanceIndex);

        void Undo(int instanceIndex, PatternInstance instance) => project.PatternInstances.Insert(instanceIndex, instance);
    }

    private void MovePatternInstance(PatternInstance instance, double time, double cropStart, double cropEnd, int lane) {
        double oldTime = instance.Time;
        double oldCropStart = instance.CropStart;
        double oldCropEnd = instance.CropEnd;
        int oldLane = instance.Lane;
        var patternInstances = project.PatternInstances;
        int fromIndex = patternInstances.IndexOf(instance);
        int toIndex = patternInstances.BinarySearch(new PatternInstance(0, time, 0d, 0d, 0));

        if (toIndex < 0)
            toIndex = ~toIndex;
        
        Do(instance, time, cropStart, cropEnd, lane);
        undoRedo.AddSubAction(() => Do(instance, oldTime, oldCropStart, oldCropEnd, oldLane), () => Do(instance, time, cropStart, cropEnd, lane));
        MoveElement(patternInstances, instance, fromIndex, toIndex);

        void Do(PatternInstance instance, double time, double cropStart, double cropEnd, int lane) {
            instance.Time = time;
            instance.CropStart = cropStart;
            instance.CropEnd = cropEnd;
            instance.Lane = lane;
        }
    }

    private void AddEventFrame(List<EventFrame> lane, EventFrame frame) => AddSortedElement(lane, frame);

    private void AddSortedElement<T>(List<T> list, T element) where T : IComparable<T> {
        int index = list.BinarySearch(element);

        if (index < 0)
            index = ~index;
        
        Do(list, index, element);
        undoRedo.AddSubAction(() => Undo(list, index), () => Do(list, index, element));

        void Do(List<T> list, int index, T element) => list.Insert(index, element);

        void Undo(List<T> list, int index) => list.RemoveAt(index);
    }

    private void MoveElement<T>(List<T> list, T element, int fromIndex, int toIndex) where T : IComparable<T> {
        Do(list, element, fromIndex, toIndex);
        undoRedo.AddSubAction(() => Do(list, element, toIndex, fromIndex), () => Do(list, element, fromIndex, toIndex));
        
        void Do(List<T> list, T element, int fromIndex, int toIndex) {
            if (toIndex > fromIndex) {
                list.Insert(toIndex, element);
                list.RemoveAt(fromIndex);
            }
            else {
                list.RemoveAt(fromIndex);
                list.Insert(toIndex, element);
            }
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
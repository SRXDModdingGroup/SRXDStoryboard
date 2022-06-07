using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryboardSystem.Editor; 

public class SongEditorView : View<SongEditorInfo> {
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject patternInstanceBlockPrefab;

    public Grid Grid => grid;
    
    private InstancePool<PatternInstanceBlock> patternInstanceBlocks;
    private int dragIndex = -1;
    private float dragStartPosition;
    private int dragStartLane;
    
    protected override void DoUpdateView() {
        var project = Info.Project;
        var patternInstances = project.PatternInstances;
        
        patternInstanceBlocks.SetCount(patternInstances.Count);

        for (int i = 0; i < patternInstances.Count; i++) {
            var patternInstance = patternInstances[i];
            var gridElement = patternInstanceBlocks[i].GridElement;
                
            gridElement.Position = (float) patternInstance.Time;
            gridElement.Lane = patternInstance.Lane;
        }
    }
    
    private void Awake() {
        patternInstanceBlocks = new InstancePool<PatternInstanceBlock>(grid.transform, patternInstanceBlockPrefab);
    }
    
    private void OnPatternInstanceBlockClick(int index, PointerEventData eventData) {
        float position = grid.ScreenXToPosition(eventData.pressPosition.x);
        int lane = grid.ScreenYToLane(eventData.pressPosition.y);
        
    }

    private void OnPatternInstanceBlockBeginDrag(int index, PointerEventData eventData) {
        dragIndex = index;
        dragStartPosition = grid.ScreenXToPosition(eventData.pressPosition.x);
        dragStartLane = grid.ScreenYToLane(eventData.pressPosition.y);
    }
    
    private void OnPatternInstanceBlockDrag(int index, PointerEventData eventData) {
        if (index != dragIndex)
            return;
        
        float endPosition = grid.ScreenXToPosition(eventData.position.x);
        int endLane = grid.ScreenYToLane(eventData.position.y);
        
    }
    
    private void OnPatternInstanceBlockEndDrag(int index, PointerEventData eventData) {
        if (index != dragIndex)
            return;
        
        float endPosition = grid.ScreenXToPosition(eventData.position.x);
        int endLane = grid.ScreenYToLane(eventData.position.y);
        
        dragIndex = -1;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryboardSystem.Editor; 

public class PatternView : ViewElement {
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform laneInfoBoxLayout;
    [SerializeField] private GameObject frameBlockPrefab;
    [SerializeField] private GameObject laneInfoBoxPrefab;

    private InstancePool<LaneInfoBox> laneInfoBoxes;
    private InstancePool<FrameBlock> frameBlocks;

    protected override void UpdateView() {
        var project = ViewInfo.Project;
        var pattern = project.Patterns[ViewInfo.ViewState.SelectedPatternIndex];
        var rigs = project.Setup.Rigs;
        var lanes = pattern.Lanes;
        int totalFrameCount = 0;
        
        laneInfoBoxes.SetCount(lanes.Count, (laneInfoBox, index) => laneInfoBox.Button.onClick.AddListener(() => OnLaneInfoBoxClicked(index)));

        for (int i = 0; i < lanes.Count; i++) {
            var lane = lanes[i];
            var rig = rigs[lane.RigIndex];
            
            laneInfoBoxes[i].RigInfoText.SetText(rig.Name);
            totalFrameCount += lane.Frames.Count;
        }
        
        frameBlocks.SetCount(totalFrameCount, (frameBlock, index) => {
            var moveHandle = frameBlock.MoveHandle;
            
            moveHandle.ClearEvents();
            moveHandle.Drag += data => OnFrameBlockDrag(index, data);
            moveHandle.BeginDrag += data => OnFrameBlockBeginDrag(index, data);
            moveHandle.EndDrag += data => OnFrameBlockEndDrag(index, data);
        });

        int frameIndex = 0;
        
        for (int i = 0; i < lanes.Count; i++) {
            var lane = lanes[i];

            foreach (var frame in lane.Frames) {
                var gridElement = frameBlocks[frameIndex].GridElement;
                
                gridElement.Position = (float) frame.Time;
                gridElement.Lane = i;
                frameIndex++;
            }
        }
    }

    private void Awake() {
        laneInfoBoxes = new InstancePool<LaneInfoBox>(laneInfoBoxLayout, laneInfoBoxPrefab);
        frameBlocks = new InstancePool<FrameBlock>(gridView.transform, frameBlockPrefab);
    }

    private void OnLaneInfoBoxClicked(int index) {
        var rig = ViewInfo.Project.Setup.Rigs[index];
    }
    
    private void OnFrameBlockDrag(int index, PointerEventData eventData) {
        float position = gridView.ScreenXToPosition(eventData.position.x);
    }

    private void OnFrameBlockBeginDrag(int index, PointerEventData eventData) {
        float position = gridView.ScreenXToPosition(eventData.position.x);
    }
    
    private void OnFrameBlockEndDrag(int index, PointerEventData eventData) {
        float position = gridView.ScreenXToPosition(eventData.position.x);
    }
}
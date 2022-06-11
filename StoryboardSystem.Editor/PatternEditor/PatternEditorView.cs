using UnityEngine;

namespace StoryboardSystem.Editor; 

public class PatternEditorView : View<PatternEditorInfo> {
    [SerializeField] private Grid grid;
    [SerializeField] private Transform laneInfoBoxLayout;
    [SerializeField] private GameObject frameBlockPrefab;
    [SerializeField] private GameObject laneInfoBoxPrefab;

    private InstancePool<LaneInfoBox> laneInfoBoxes;
    private InstancePool<FrameBlock> frameBlocks;

    protected override void DoUpdateView() {
        var project = Info.Project;
        var pattern = project.Patterns[Info.State.SelectedPatternIndex];
        var rigs = project.Setup.Rigs;
        var lanes = pattern.Lanes;
        int totalFrameCount = 0;
        
        laneInfoBoxes.SetCount(lanes.Count, (laneInfoBox, index) => laneInfoBox.Button.onClick.AddListener(() => OnLaneInfoBoxClicked(index)));

        for (int i = 0; i < lanes.Count; i++) {
            laneInfoBoxes[i].RigInfoText.SetText(rigs[i].Name);
            totalFrameCount += lanes[i].Frames.Count;
        }
        
        frameBlocks.SetCount(totalFrameCount);

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
        frameBlocks = new InstancePool<FrameBlock>(grid.transform, frameBlockPrefab);
    }

    private void OnLaneInfoBoxClicked(int index) {
        
    }
}
using UnityEngine;

namespace VisualizerSystem.Editor; 

public class SongEditorView : View<SongEditorInfo> {
    [SerializeField] private Grid grid;
    [SerializeField] private Transform laneInfoBoxLayout;
    [SerializeField] private GameObject frameBlockPrefab;
    [SerializeField] private GameObject laneInfoBoxPrefab;

    public Grid Grid => grid;

    private InstancePool<LaneInfoBox> laneInfoBoxes;
    private InstancePool<FrameBlock> frameBlocks;

    protected override void DoUpdateView() {
        var project = Info.Project;
        var rigs = project.Setup.Rigs;
        var lanes = project.Lanes;
        int totalFrameCount = 0;

        grid.LaneCount = lanes.Count;
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
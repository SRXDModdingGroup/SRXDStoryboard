using UnityEngine;

namespace StoryboardSystem.Editor; 

public class PatternView : View {
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform laneInfoBoxLayout;
    [SerializeField] private GameObject frameBlockPrefab;
    [SerializeField] private GameObject laneInfoBoxPrefab;

    private InstancePool<LaneInfoBox> laneInfoBoxes;
    private InstancePool<GridElement> frameBlocks;

    public void UpdateInfo(ProjectSetup setup, Pattern pattern) {
        var lanes = pattern.Lanes;
        int totalFrameCount = 0;
        
        laneInfoBoxes.SetCount(lanes.Count);

        for (int i = 0; i < lanes.Count; i++) {
            var lane = lanes[i];
            var rig = setup.Rigs[lane.RigIndex];
            
            laneInfoBoxes[i].UpdateInfo(rig, lane);
            totalFrameCount += lane.Frames.Count;
        }
        
        frameBlocks.SetCount(totalFrameCount);

        int frameIndex = 0;
        
        for (int i = 0; i < lanes.Count; i++) {
            var lane = lanes[i];

            foreach (var frame in lane.Frames) {
                var frameBlock = frameBlocks[frameIndex];
                
                frameBlock.Position = (float) frame.Time;
                frameBlock.Lane = i;
                frameIndex++;
            }
        }
        
        ScheduleUpdate();
    }

    protected override void UpdateView() {
        gridView.ScheduleUpdate();
    }

    private void Awake() {
        laneInfoBoxes = new InstancePool<LaneInfoBox>(laneInfoBoxLayout, laneInfoBoxPrefab);
        frameBlocks = new InstancePool<GridElement>(gridView.transform, frameBlockPrefab);
    }
}
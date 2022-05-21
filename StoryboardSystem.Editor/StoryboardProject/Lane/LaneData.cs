namespace StoryboardSystem.Editor; 

public struct LaneData {
    public int First { get; set; }
    
    public int ClusterCount { get; set; }
    
    public int ClusterStep { get; set; }
    
    public int SequenceCount { get; set; }
    
    public int SequenceStep { get; set; }
    
    public LaneData() {
        First = 0;
        ClusterCount = 1;
        ClusterStep = 1;
        SequenceCount = 1;
        SequenceStep = 1;
    }

    public LaneData(int first, int clusterCount, int clusterStep, int sequenceCount, int sequenceStep) {
        First = first;
        ClusterCount = clusterCount;
        ClusterStep = clusterStep;
        SequenceCount = sequenceCount;
        SequenceStep = sequenceStep;
    }
}
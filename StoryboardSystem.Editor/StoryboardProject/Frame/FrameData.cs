﻿namespace StoryboardSystem.Editor; 

public struct FrameData {
    public int First { get; set; }
    
    public int ClusterCount { get; set; }
    
    public int ClusterStep { get; set; }
    
    public int SequenceCount { get; set; }
    
    public int SequenceStep { get; set; }
    
    public SequenceTimeType SequenceTimeType { get; set; }
    
    public double SequenceTime { get; set; }
    
    public int SequenceRepeat { get; set; }

    public FrameData() {
        First = 0;
        ClusterCount = 1;
        ClusterStep = 1;
        SequenceCount = 1;
        SequenceStep = 1;
        SequenceTimeType = SequenceTimeType.Interval;
        SequenceTime = 0d;
        SequenceRepeat = 1;
    }

    public FrameData(int first, int clusterCount, int clusterStep, int sequenceCount, int sequenceStep, SequenceTimeType sequenceTimeType, double sequenceTime, int sequenceRepeat) {
        First = first;
        ClusterCount = clusterCount;
        ClusterStep = clusterStep;
        SequenceCount = sequenceCount;
        SequenceStep = sequenceStep;
        SequenceTimeType = sequenceTimeType;
        SequenceTime = sequenceTime;
        SequenceRepeat = sequenceRepeat;
    }
}
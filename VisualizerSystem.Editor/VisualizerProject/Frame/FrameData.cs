namespace VisualizerSystem.Editor; 

public struct FrameData {
    public int SequenceCount { get; set; }
    
    public SequenceTimeType SequenceTimeType { get; set; }
    
    public double SequenceTime { get; set; }

    public FrameData() {
        SequenceCount = 1;
        SequenceTimeType = SequenceTimeType.Interval;
        SequenceTime = 0d;
    }

    public FrameData(int sequenceCount, SequenceTimeType sequenceTimeType, double sequenceTime, int sequenceRepeat) {
        SequenceCount = sequenceCount;
        SequenceTimeType = sequenceTimeType;
        SequenceTime = sequenceTime;
    }
}
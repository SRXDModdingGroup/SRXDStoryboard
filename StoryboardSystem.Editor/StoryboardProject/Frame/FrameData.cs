namespace StoryboardSystem.Editor; 

public struct FrameData {
    public SequenceTimeType SequenceTimeType { get; set; }
    
    public double SequenceTime { get; set; }
    
    public int SequenceRepeat { get; set; }

    public FrameData() {
        SequenceTimeType = SequenceTimeType.Interval;
        SequenceTime = 0d;
        SequenceRepeat = 1;
    }

    public FrameData(SequenceTimeType sequenceTimeType, double sequenceTime, int sequenceRepeat) {
        SequenceTimeType = sequenceTimeType;
        SequenceTime = sequenceTime;
        SequenceRepeat = sequenceRepeat;
    }
}
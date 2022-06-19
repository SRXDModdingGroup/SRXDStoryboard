namespace VisualizerSystem.Editor {
    public struct FrameData {
        public int SequenceCount { get; set; }
    
        public SequenceTimeType SequenceTimeType { get; set; }
    
        public double SequenceTime { get; set; }

        public FrameData(int sequenceCount, SequenceTimeType sequenceTimeType, double sequenceTime) {
            SequenceCount = sequenceCount;
            SequenceTimeType = sequenceTimeType;
            SequenceTime = sequenceTime;
        }
    }
}
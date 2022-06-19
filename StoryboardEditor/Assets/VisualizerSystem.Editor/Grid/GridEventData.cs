namespace VisualizerSystem.Editor {
    public class GridEventData {
        public double Position { get; }
    
        public int Lane { get; }
    
        public double DragStartPosition { get; }
    
        public int DragStartLane { get; }

        public GridEventData(double position, int lane, double dragStartPosition, int dragStartLane) {
            Position = position;
            Lane = lane;
            DragStartPosition = dragStartPosition;
            DragStartLane = dragStartLane;
        }
    }
}
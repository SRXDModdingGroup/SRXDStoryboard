namespace VisualizerSystem.Editor; 

public class GridEventData {
    public float Position { get; }
    
    public int Lane { get; }
    
    public float DragStartPosition { get; }
    
    public float DragStartLane { get; }
    
    public int HoveredElementIndex { get; }
    
    public int HoveredHandleIndex { get; }

    public GridEventData(float position, int lane, float dragStartPosition, float dragStartLane, int hoveredElementIndex, int hoveredHandleIndex) {
        Position = position;
        Lane = lane;
        DragStartPosition = dragStartPosition;
        DragStartLane = dragStartLane;
        HoveredElementIndex = hoveredElementIndex;
        HoveredHandleIndex = hoveredHandleIndex;
    }
}
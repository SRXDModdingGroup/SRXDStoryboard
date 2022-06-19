namespace VisualizerSystem.Editor {
    public abstract class SongEditorTool {
        public virtual void OnClick(GridEventData data, VisualizerModel model, SongEditorState state) { }
    
        public virtual void OnDrag (GridEventData data, VisualizerModel model, SongEditorState state) { }
    
        public virtual void OnBeginDrag(GridEventData data, VisualizerModel model, SongEditorState state) { }
    
        public virtual void OnEndDrag(GridEventData data, VisualizerModel model, SongEditorState state) { }
    }
}
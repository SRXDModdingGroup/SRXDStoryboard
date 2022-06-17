namespace VisualizerSystem.Editor; 

public class SongEditorInfo {
    public VisualizerProject Project { get; }
    
    public SongEditorState State { get; }

    public SongEditorInfo(VisualizerProject project, SongEditorState state) {
        Project = project;
        State = state;
    }
}
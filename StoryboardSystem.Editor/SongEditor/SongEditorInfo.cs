namespace StoryboardSystem.Editor; 

public class SongEditorInfo {
    public StoryboardProject Project { get; }
    
    public SongEditorState State { get; }

    public SongEditorInfo(StoryboardProject project, SongEditorState state) {
        Project = project;
        State = state;
    }
}
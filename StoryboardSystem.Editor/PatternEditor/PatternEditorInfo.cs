namespace StoryboardSystem.Editor; 

public class PatternEditorInfo {
    public StoryboardProject Project { get; }
    
    public PatternEditorState State { get; }

    public PatternEditorInfo(StoryboardProject project, PatternEditorState state) {
        Project = project;
        State = state;
    }
}
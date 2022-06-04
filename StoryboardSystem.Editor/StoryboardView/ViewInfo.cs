namespace StoryboardSystem.Editor; 

public class ViewInfo {
    public StoryboardProject Project { get; }
    
    public StoryboardEditor Editor { get; }
    
    public ViewState ViewState { get; }

    public ViewInfo(StoryboardProject project, StoryboardEditor editor, ViewState viewState) {
        Project = project;
        Editor = editor;
        ViewState = viewState;
    }
}
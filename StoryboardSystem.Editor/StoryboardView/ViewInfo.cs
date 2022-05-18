namespace StoryboardSystem.Editor; 

public struct ViewInfo {
    public StoryboardProject Project { get; }
    
    public bool CanUndo { get; }
    
    public bool CanRedo { get; }

    public ViewInfo(StoryboardProject project, bool canUndo, bool canRedo) {
        Project = project;
        CanUndo = canUndo;
        CanRedo = canRedo;
    }
}
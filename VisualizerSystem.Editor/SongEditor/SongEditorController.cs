using System;

namespace VisualizerSystem.Editor; 

public class SongEditorController : Controller<SongEditorView, SongEditorInfo> {
    private SongEditorState state = new();
    private SongEditorTool[] tools = {
        new SongEditorAddTool()
    };

    protected override SongEditorInfo CreateViewInfo() => new(Model.Project, state);

    protected override void Awake() {
        base.Awake();

        var grid = View.Grid;

        grid.Click += data => GetCurrentTool.OnClick(data, Model, state);
        grid.Drag += data => GetCurrentTool.OnDrag(data, Model, state);
        grid.BeginDrag += data => GetCurrentTool.OnBeginDrag(data, Model, state);
        grid.EndDrag += data => GetCurrentTool.OnEndDrag(data, Model, state);
    }

    private SongEditorTool GetCurrentTool => tools[state.SelectedToolIndex];
}
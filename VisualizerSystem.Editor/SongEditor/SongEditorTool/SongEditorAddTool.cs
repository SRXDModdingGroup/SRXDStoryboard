using System.Collections.Generic;

namespace VisualizerSystem.Editor; 

public class SongEditorAddTool : SongEditorTool {
    public override void OnClick(GridEventData data, VisualizerModel model, SongEditorState state) {
        using var edit = model.CreateEditBlock();
        var lane = model.Project.Lanes[data.Lane];
        
        edit.AddFrame(lane, new Frame(data.Position, new FrameData(), new List<ValueData>()));
    }
}
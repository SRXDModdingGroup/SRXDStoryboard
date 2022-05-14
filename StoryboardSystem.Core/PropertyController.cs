using System.Collections.Generic;

namespace StoryboardSystem.Core; 

public class PropertyController {
    public PropertyBinding Binding { get; }
    
    public List<Curve> Curves { get; }

    private int lastEvaluatedCurveIndex;
    private int lastEvaluatedKeyframeIndex;

    public PropertyController(PropertyBinding binding, List<Curve> curves) {
        Binding = binding;
        Curves = curves;
    }

    public void Evaluate(double time) {
        int curveIndex = lastEvaluatedCurveIndex;

        if (curveIndex < 0)
            curveIndex = 0;

        while (true) {
            if (curveIndex < Curves.Count - 1 && time >= Curves[curveIndex + 1].StartTime) {
                curveIndex++;
                
                continue;
            }

            var curve = Curves[curveIndex];

            if (curveIndex > 0 && time < curve.StartTime) {
                curveIndex--;
                
                continue;
            }

            if (curveIndex != lastEvaluatedCurveIndex) {
                lastEvaluatedCurveIndex = curveIndex;
                lastEvaluatedKeyframeIndex = 0;
            }

            Binding.Set(curve.Evaluate(time, lastEvaluatedKeyframeIndex));

            return;
        }
    }
}
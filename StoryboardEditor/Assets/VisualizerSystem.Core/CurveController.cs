using System.Collections.Generic;
using VisualizerSystem.Rigging;

namespace VisualizerSystem.Core {
    public class CurveController {
        public VisualizerRig Rig { get; }
    
        public List<Curve> Curves { get; }

        private int lastEvaluatedCurveIndex;
        private int lastEvaluatedKeyframeIndex;

        public CurveController(VisualizerRig rig, List<Curve> curves) {
            Rig = rig;
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

                Rig.Execute(curve.Evaluate(time, ref lastEvaluatedKeyframeIndex));

                return;
            }
        }
    }
}


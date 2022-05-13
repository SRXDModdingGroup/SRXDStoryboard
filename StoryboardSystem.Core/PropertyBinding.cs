using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryboardSystem.Core; 

public class PropertyBinding {
    public List<Action<Vector4>> Actions { get; }
    
    public List<Curve> Curves { get; }

    private int lastEvaluatedCurveIndex;
    private int lastEvaluatedKeyframeIndex;

    public PropertyBinding(List<Action<Vector4>> actions, List<Curve> curves) {
        Actions = actions;
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

            foreach (var action in Actions)
                action(curve.Evaluate(time, lastEvaluatedKeyframeIndex));

            return;
        }
    }
}
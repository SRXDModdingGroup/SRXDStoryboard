using System.Collections.Generic;
using VisualizerSystem.Rigging;

namespace VisualizerSystem.Core {
    public class EventController {
        public VisualizerRig Rig { get; }
    
        public List<Keyframe> Keyframes { get; }

        private int lastCallIndex;

        public EventController(VisualizerRig rig, List<Keyframe> keyframes) {
            Rig = rig;
            Keyframes = keyframes;
        }

        public void Evaluate(double time, bool trigger) {
            while (true) {
                if (lastCallIndex < Keyframes.Count - 1 && time >= Keyframes[lastCallIndex + 1].Time) {
                    lastCallIndex++;

                    if (!trigger)
                        continue;
                
                    Rig.Execute(Keyframes[lastCallIndex].Parameters);
                }
                else if (lastCallIndex >= 0 && time < Keyframes[lastCallIndex].Time)
                    lastCallIndex--;
                else
                    return;
            }
        }
    }
}
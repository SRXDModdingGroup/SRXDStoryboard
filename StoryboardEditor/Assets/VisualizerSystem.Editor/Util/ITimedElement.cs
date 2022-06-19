using System.Collections.Generic;

namespace VisualizerSystem.Editor {
    public interface ITimedElement {
        public double Time { get; }
    }

    public static class ITimedElementExtensions {
        public static int BinarySearch<T>(this IList<T> list, double time) where T : ITimedElement {
            if (list.Count == 0)
                return 0;
        
            if (time <= list[0].Time)
                return 0;
        
            if (time >= list[list.Count - 1].Time)
                return list.Count;
        
            int start = 0;
            int end = list.Count - 1;

            while (start <= end) {
                int mid = (start + end) / 2;
                double midTime = list[mid].Time;

                if (midTime <= time)
                    start = mid + 1;
                else
                    end = mid - 1;
            }
        
            return start;
        }
    }
}
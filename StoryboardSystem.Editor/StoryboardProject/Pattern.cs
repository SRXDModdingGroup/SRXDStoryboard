using System.Collections.Generic;

namespace StoryboardSystem.Editor; 

public class Pattern {
    public string Name { get; set; }
    
    public List<Lane> Lanes { get; }

    public Pattern(string name, int laneCount) {
        Name = name;
        Lanes = new List<Lane>(laneCount);

        for (int i = 0; i < laneCount; i++)
            Lanes.Add(new Lane());
    }

    public double GetLength() {
        double length = 0d;

        foreach (var lane in Lanes) {
            foreach (var frame in lane.Frames) {
                if (frame.Time > length)
                    length = frame.Time;
            }
        }

        return length;
    }
}
namespace StoryboardSystem.Editor; 

public class Pattern {
    public string Name { get; set; }
    
    public Channel[] Channels { get; }

    public Pattern(string name, ProjectSetup setup) {
        Name = name;
        
        var rigs = setup.Rigs;
        
        Channels = new Channel[rigs.Length];

        for (int i = 0; i < rigs.Length; i++)
            Channels[i] = new Channel(rigs[i]);
    }

    public double GetLength() {
        double length = 0d;

        foreach (var channel in Channels) {
            foreach (var frames in channel.EventLanes) {
                double maxTime = Frame.GetMaxTime(frames);

                if (maxTime > length)
                    length = maxTime;
            }
            
            foreach (var frames in channel.PropertyLanes) {
                double maxTime = Frame.GetMaxTime(frames);

                if (maxTime > length)
                    length = maxTime;
            }
        }

        return length;
    }
}
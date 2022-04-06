namespace StoryboardSystem; 

internal class Index {
    public object[] Array { get; }
    
    public int index { get; }

    public Index(object[] array, int index) {
        Array = array;
        this.index = index;
    }
}
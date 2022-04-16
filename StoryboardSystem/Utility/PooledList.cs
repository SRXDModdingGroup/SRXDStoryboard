using System;
using System.Collections.Generic;

namespace StoryboardSystem; 

public class PooledList : List<object>, IDisposable {
    private static Queue<PooledList> freeLists = new();

    private bool free;
    
    public static PooledList Get() {
        if (freeLists.Count == 0)
            return new PooledList();

        return freeLists.Dequeue();
    }
    
    public void Dispose() {
        if (free)
            return;

        free = true;
        Clear();
        freeLists.Enqueue(this);
    }

    ~PooledList() => Dispose();
}
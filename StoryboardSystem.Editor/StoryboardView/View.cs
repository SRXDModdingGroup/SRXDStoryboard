using UnityEngine;

namespace StoryboardSystem.Editor; 

public abstract class View : MonoBehaviour {
    private bool dirty;

    public void ScheduleUpdate() => dirty = true;
    
    protected void LateUpdate() {
        if (!dirty)
            return;

        dirty = false;
        UpdateView();
    }

    protected abstract void UpdateView();
}
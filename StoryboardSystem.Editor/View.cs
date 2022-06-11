using UnityEngine;

namespace StoryboardSystem.Editor;

public abstract class View : MonoBehaviour {
    private bool needsUpdate;

    public void UpdateView() => needsUpdate = true;
    
    protected abstract void DoUpdateView();
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void LateUpdate() {
        if (!needsUpdate)
            return;

        needsUpdate = false;
        DoUpdateView();
    }
}

public abstract class View<T> : View {
    protected T Info { get; private set; }

    public void UpdateView(T info) {
        Info = info;
        UpdateView();
    }
}
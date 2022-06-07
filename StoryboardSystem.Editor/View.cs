using UnityEngine;

namespace StoryboardSystem.Editor;

public abstract class View : MonoBehaviour {
    private bool needsUpdate;
    
    public void UpdateView() {
        if (gameObject.activeInHierarchy) {
            needsUpdate = false;
            DoUpdateView();
        }
        else
            needsUpdate = true;
    }
    
    protected abstract void DoUpdateView();
    
    private void OnEnable() {
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
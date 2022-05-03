using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Popup : MonoBehaviour {
    private InputBlocker blocker;

    public virtual void Show(InputBlocker blocker) {
        this.blocker = blocker;
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(gameObject);
        blocker.Activate(gameObject);
        blocker.Click += Hide;
    }
    
    public virtual void Hide() {
        gameObject.SetActive(false);

        if (EventSystem.current.gameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
        
        if (blocker == null)
            return;
        
        blocker.Deactivate(gameObject);
        blocker.Click -= Hide;
        blocker = null;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputBlocker : MonoBehaviour, IPointerClickHandler {
    private HashSet<GameObject> sources = new();

    public event Action Click; 

    public void Activate(GameObject source) {
        sources.Add(source);
        gameObject.SetActive(true);
    }

    public void Deactivate(GameObject source) {
        sources.Remove(source);

        if (sources.Count == 0)
            gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) => Click?.Invoke();
}

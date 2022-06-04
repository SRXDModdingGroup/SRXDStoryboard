using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryboardSystem.Editor; 

public class GrabHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    public event Action<PointerEventData> Drag;
    public event Action<PointerEventData> BeginDrag;
    public event Action<PointerEventData> EndDrag;

    public void ClearEvents() {
        Drag = null;
        BeginDrag = null;
        EndDrag = null;
    }
    
    public void OnDrag(PointerEventData eventData) => Drag?.Invoke(eventData);

    public void OnBeginDrag(PointerEventData eventData) => BeginDrag?.Invoke(eventData);

    public void OnEndDrag(PointerEventData eventData) => EndDrag?.Invoke(eventData);
}
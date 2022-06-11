using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryboardSystem.Editor; 

public class Grid : View, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, IDeselectHandler {
    [SerializeField] private float laneHeight;
    
    public event Action<GridEventData> Click;
    public event Action<GridEventData> Drag;
    public event Action<GridEventData> BeginDrag;
    public event Action<GridEventData> EndDrag;

    public float Scroll {
        get => scroll;
        set {
            scroll = value;
            DoUpdateView();
        }
    }

    public float Scale {
        get => scale;
        set {
            scale = value;
            DoUpdateView();
        }
    }

    private float scroll;
    private float scale = 1f;
    private bool dragging;
    private float dragStartPosition;
    private int dragStartLane;
    private RectTransform rectTransform;
    private List<GridElement> elements = new();

    public void AddElement(GridElement element) => elements.Add(element);

    public void RemoveElement(GridElement element) => elements.Remove(element);

    public void OnPointerClick(PointerEventData eventData) {
        (float position, int lane, var pointerPosition) = GetDataFromPointerEvent(eventData);

        dragging = false;
        dragStartPosition = position;
        dragStartLane = lane;
        Click?.Invoke(CreateGridEventData(position, lane, pointerPosition));
    }

    public void OnDrag(PointerEventData eventData) {
        if (!dragging)
            return;
        
        (float position, int lane, var pointerPosition) = GetDataFromPointerEvent(eventData);

        Drag?.Invoke(CreateGridEventData(position, lane, pointerPosition));
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (dragging)
            CancelDrag();
        
        (float position, int lane, var pointerPosition) = GetDataFromPointerEvent(eventData);

        dragging = true;
        dragStartPosition = position;
        dragStartLane = lane;
        BeginDrag?.Invoke(CreateGridEventData(position, lane, pointerPosition));
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!dragging)
            return;
        
        (float position, int lane, var pointerPosition) = GetDataFromPointerEvent(eventData);

        dragging = false;
        EndDrag?.Invoke(CreateGridEventData(position, lane, pointerPosition));
    }

    public void OnScroll(PointerEventData eventData) {
        throw new NotImplementedException();
    }
    
    public void OnDeselect(BaseEventData eventData) => CancelDrag();

    public bool IsInVisibleBounds(float startPosition, float endPosition) => startPosition <= scroll + 1f / scale && endPosition >= scroll;

    public int LocalYToLane(float y) => Mathf.FloorToInt(-y / laneHeight);

    public int ScreenYToLane(float y) => LocalYToLane(y - rectTransform.anchoredPosition.y);

    public float LocalXToPosition(float x) => x / (rectTransform.sizeDelta.x * scale) + scroll;

    public float ScreenXToPosition(float x) => LocalXToPosition(x - rectTransform.anchoredPosition.x);

    public Vector2 GridToLocalSpace(float position, int lane) => new(rectTransform.sizeDelta.x * scale * (position - scroll), -laneHeight * lane);

    protected override void DoUpdateView() {
        foreach (var element in elements)
            element.UpdateView();
    }

    private void Awake() => rectTransform = GetComponent<RectTransform>();

    private void CancelDrag() {
        if (!dragging)
            return;
        
        dragging = false;
    }

    private (float, int, Vector2) GetDataFromPointerEvent(PointerEventData eventData) {
        var pointerPosition = eventData.position;

        return (ScreenXToPosition(pointerPosition.x), ScreenYToLane(pointerPosition.y), pointerPosition);
    }

    private GridEventData CreateGridEventData(float position, int lane, Vector2 pointerPosition) {
        (int elementIndex, int handleIndex) = GetElementAndHandleAtPosition(pointerPosition);

        return new GridEventData(position, lane, dragStartPosition, dragStartLane, elementIndex, handleIndex);
    }

    private (int elementIndex, int handleIndex) GetElementAndHandleAtPosition(Vector2 position) {
        for (int i = 0; i < elements.Count; i++) {
            var handles = elements[i].Handles;

            for (int j = 0; j < handles.Count; j++) {
                if (handles[i].rect.Contains(position))
                    return (i, j);
            }
        }

        return (-1, -1);
    }
}
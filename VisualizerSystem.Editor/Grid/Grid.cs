using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VisualizerSystem.Editor; 

public class Grid : View, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, IDeselectHandler {
    [SerializeField] private float laneHeight;
    
    public event Action<GridEventData> Click;
    public event Action<GridEventData> Drag;
    public event Action<GridEventData> BeginDrag;
    public event Action<GridEventData> EndDrag;

    public double Scroll {
        get => scroll;
        set {
            scroll = value;
            UpdateView();
        }
    }

    public double Scale {
        get => scale;
        set {
            scale = value;
            UpdateView();
        }
    }

    public int LaneCount {
        get => laneCount;
        set {
            laneCount = value;
            UpdateView();
        }
    }

    private double scroll;
    private double scale = 1d;
    private int laneCount;
    private bool dragging;
    private double dragStartPosition;
    private int dragStartLane;
    private RectTransform rectTransform;
    private List<GridElement> elements = new();

    public void AddElement(GridElement element) => elements.Add(element);

    public void RemoveElement(GridElement element) => elements.Remove(element);

    public void OnPointerClick(PointerEventData eventData) {
        if (!TryGetDataFromPointerEvent(eventData, out double position, out int lane))
            return;

        dragging = false;
        dragStartPosition = position;
        dragStartLane = lane;
        Click?.Invoke(CreateGridEventData(position, lane));
    }

    public void OnDrag(PointerEventData eventData) {
        if (!dragging)
            return;

        TryGetDataFromPointerEvent(eventData, out double position, out int lane);
        Drag?.Invoke(CreateGridEventData(position, lane));
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (dragging)
            CancelDrag();
        
        if (!TryGetDataFromPointerEvent(eventData, out double position, out int lane))
            return;

        dragging = true;
        dragStartPosition = position;
        dragStartLane = lane;
        BeginDrag?.Invoke(CreateGridEventData(position, lane));
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!dragging)
            return;

        TryGetDataFromPointerEvent(eventData, out double position, out int lane);
        dragging = false;
        EndDrag?.Invoke(CreateGridEventData(position, lane));
    }

    public void OnScroll(PointerEventData eventData) {
        throw new NotImplementedException();
    }
    
    public void OnDeselect(BaseEventData eventData) => CancelDrag();

    public bool IsInVisibleBounds(double startPosition, double endPosition) => startPosition <= scroll + 1f / scale && endPosition >= scroll;

    public int LocalYToLane(float y) => Mathf.FloorToInt(-y / laneHeight);

    public int ScreenYToLane(float y) => LocalYToLane(y - rectTransform.anchoredPosition.y);

    public double LocalXToPosition(float x) => x / (rectTransform.sizeDelta.x * scale) + scroll;

    public double ScreenXToPosition(float x) => LocalXToPosition(x - rectTransform.anchoredPosition.x);

    public Vector2 GridToLocalSpace(double position, int lane) => new((float) (rectTransform.sizeDelta.x * scale * (position - scroll)), -laneHeight * lane);

    public override void UpdateView() {
        base.UpdateView();
        
        foreach (var element in elements)
            element.UpdateView();
    }

    private void Awake() => rectTransform = GetComponent<RectTransform>();

    private void CancelDrag() {
        if (!dragging)
            return;
        
        dragging = false;
    }

    private bool TryGetDataFromPointerEvent(PointerEventData eventData, out double position, out int lane) {
        var pointerPosition = eventData.position;

        position = ScreenXToPosition(pointerPosition.x);
        lane = ScreenYToLane(pointerPosition.y);

        bool inBounds = true;

        if (lane < 0) {
            inBounds = false;
            lane = 0;
        }
        else if (lane >= laneCount) {
            inBounds = false;
            lane = laneCount - 1;
        }

        return inBounds;
    }

    private GridEventData CreateGridEventData(double position, int lane) => new(position, lane, dragStartPosition, dragStartLane);
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryboardSystem.Editor; 

public class GridView : ViewElement, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler {
    [SerializeField] private float laneHeight;
    
    public float Scroll {
        get => scroll;
        set {
            scroll = value;
            ViewChanged?.Invoke();
        }
    }

    public float Scale {
        get => scale;
        set {
            scale = value;
            ViewChanged?.Invoke();
        }
    }

    public event Action ViewChanged; 

    private float scroll;
    private float scale = 1f;
    private RectTransform rectTransform;

    public void OnPointerClick(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public void OnScroll(PointerEventData eventData) {
        throw new NotImplementedException();
    }

    public int LocalYToLane(float y) => Mathf.FloorToInt(-y / laneHeight);

    public int ScreenYToLane(float y) => LocalYToLane(y - rectTransform.anchoredPosition.y);

    public float LocalXToPosition(float x) => x / (rectTransform.sizeDelta.x * scale) + scroll;

    public float ScreenXToPosition(float x) => LocalXToPosition(x - rectTransform.anchoredPosition.x);

    public Vector2 GridToLocalSpace(float position, int lane) => new(rectTransform.sizeDelta.x * scale * (position - scroll), -laneHeight * lane);

    protected override void UpdateView() {
        ViewChanged?.Invoke();
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }
}
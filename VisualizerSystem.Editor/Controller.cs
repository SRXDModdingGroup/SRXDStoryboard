using UnityEngine;

namespace VisualizerSystem.Editor;

public abstract class Controller<TView, TInfo> : MonoBehaviour where TView : View<TInfo> {
    [SerializeField] private VisualizerModel model;
    [SerializeField] private TView view;

    protected abstract TInfo CreateViewInfo();

    private void Awake() => model.Changed += OnModelChanged;

    private void OnModelChanged() => view.UpdateView(CreateViewInfo());
}
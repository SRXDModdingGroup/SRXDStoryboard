using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarButton : MonoBehaviour {
    [SerializeField] private BindableAction[] actions;
    [SerializeField] private Button button;
    [SerializeField] private ContextMenu contextMenu;

    public BindableAction[] Actions => actions;

    private Action<BindableAction> callback;

    public void Init(List<ContextMenu.StringPair> values, Action<BindableAction> callback, Button blocker) {
        contextMenu.SetValues(values);
        contextMenu.Init(blocker);
        this.callback = callback;
    }
    
    private void Awake() => button.onClick.AddListener(OnButtonClick);

    private void OnButtonClick() => contextMenu.Show(OnContextMenuOptionSelected);

    private void OnContextMenuOptionSelected(int index) => callback(actions[index]);
}
